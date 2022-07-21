﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace FreeNetLite;

public class Session
{
    const int STATE_IDLE = 0;
    const int STATE_CONNECTED = 1;
    const int STATE_CLOSED = 2;

    public UInt64 UniqueId { get; private set; } = 0;

    ServerOption ServerOpt;

    int CurrentState = STATE_IDLE;

    public Socket Sock { get; set; }

    public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
    public SocketAsyncEventArgs SendEventArgs { get; private set; }

    List<ArraySegment<byte>> SendingList;
    object LOCK_SENDING_QUEUE;

    IPacketDispatcher Dispatcher;

    public Action<Session> OnEventSessionClosed;
    public Action<object, SocketAsyncEventArgs> OnEventSendCompleted;


    public Session(bool isClient, UInt64 uniqueId, IPacketDispatcher dispatcher, ServerOption serverOption)
    {
        UniqueId = uniqueId;
        Dispatcher = dispatcher;
        ServerOpt = serverOption;

        SendingList = new List<ArraySegment<byte>>();
        LOCK_SENDING_QUEUE = new object();
    }

    public void OnConnected()
    {
        CurrentState = STATE_CONNECTED;

        var msg = new Packet(this, (UInt16)NetworkDefine.SYS_NTF_CONNECTED, null);
        Dispatcher.IncomingPacket(true, this, msg);
    }

    public void SetEventArgs(SocketAsyncEventArgs receive_event_args, SocketAsyncEventArgs send_event_args)
    {
        ReceiveEventArgs = receive_event_args;
        SendEventArgs = send_event_args;
    }

    public void OnReceive(byte[] buffer, int offset, int transfered)
    {
        Dispatcher.DispatchPacket(this, buffer, offset, transfered);
    }

    public void Close()
    {
        // 중복 수행을 막는다.
        if (Interlocked.Exchange(ref CurrentState, STATE_CLOSED) == STATE_CLOSED)
        {
            return;
        }

        CurrentState = STATE_CLOSED;

        Sock.Close();
        Sock = null;

        SendEventArgs.UserToken = null;
        ReceiveEventArgs.UserToken = null;

        SendingList.Clear();

        OnEventSessionClosed(this);

        var msg = new Packet(this, (UInt16)NetworkDefine.SYS_NTF_CLOSED, null);
        Dispatcher.IncomingPacket(true, this, msg);
    }

    public void Send(ArraySegment<byte> packetData)
    {
        PostSend(packetData);
    }

    /// 비동기 전송 완료시 호출되는 콜백 매소드.
    public bool SendCompleted(SocketAsyncEventArgs e)
    {
        if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
        {
            // 연결이 끊겨서 이미 소켓이 종료된 경우일 것이다.
            //Console.WriteLine(string.Format("Failed to send. error {0}, transferred {1}", e.SocketError, e.BytesTransferred));
            return false;
        }


        lock (this.LOCK_SENDING_QUEUE)
        {
            // 리스트에 들어있는 데이터의 총 바이트 수.
            var size = SendingList.Sum(obj => obj.Count);

            // MTU 이하로만 보내므로 이 조건문 안에 들어올 수 있다.
            // 전송이 완료되기 전에 추가 전송 요청을 했다면 sending_list에 무언가 더 들어있을 것이다.
            if (e.BytesTransferred != size)
            {
                // 보낸 만큼 빼고 나머지 대기중인 데이터들을 한방에 보내버린다.
                int sent_index = 0;
                int sum = 0;
                for (int i = 0; i < SendingList.Count; ++i)
                {
                    sum += SendingList[i].Count;
                    if (sum <= e.BytesTransferred)
                    {
                        // 여기 까지는 전송 완료된 데이터 인덱스.
                        sent_index = i;
                        continue;
                    }

                    break;
                }

                // 전송 완료된것은 리스트에서 삭제한다.
                SendingList.RemoveRange(0, sent_index + 1);

                if (SendingList.Count > 0)
                {
                    // 나머지 데이터들을 한방에 보낸다.
                    AsyncSendIO(SendingList);
                }
            }
            else
            {
                // 다 보냈고 더이상 보낼것도 없다.
                SendingList.Clear();
            }
        }

        return true;
    }

    public bool IsConnected() => CurrentState == STATE_CONNECTED;


    /// 패킷을 전송한다.
    /// 큐가 비어 있을 경우에는 큐에 추가한 뒤 바로 SendAsync매소드를 호출하고,
    /// 데이터가 들어있을 경우에는 새로 추가만 한다.
    /// 
    /// 큐잉된 패킷의 전송 시점 :
    ///		현재 진행중인 SendAsync가 완료되었을 때 큐를 검사하여 나머지 패킷을 전송한다.
    void PostSend(ArraySegment<byte> data)
    {
        if(IsConnected() == false)
        {
            return;
        }
                    
        lock (this.LOCK_SENDING_QUEUE)
        {
            SendingList.Add(data);

            if (SendingList.Count > 1)
            {
                // 큐에 무언가가 들어 있다면 아직 이전 전송이 완료되지 않은 상태이므로 큐에 추가만 하고 리턴한다.
                // 현재 수행중인 SendAsync가 완료된 이후에 큐를 검사하여 데이터가 있으면 SendAsync를 호출하여 전송해줄 것이다.
                return;
            }

            AsyncSendIO(SendingList);
        }                        
    }

    /// 비동기 전송을 시작한다.
    void AsyncSendIO(List<ArraySegment<byte>> sendingList)
    {
        if (IsConnected() == false)
        {
            return;
        }

        try
        {
            //SendEventArgs.BufferList = sendingList;
            // MTU 사이즈 이내만 보내도록 한다
            {
                List<ArraySegment<byte>> tempList = new();
                var dataSize = 0;
                
                foreach (var sendInfo in sendingList)
                {
                    if ((dataSize + sendInfo.Count) <= ServerOpt.MaxPacketSize)
                    {
                        dataSize += sendInfo.Count;

                        tempList.Add(sendInfo);
                    }
                }

                SendEventArgs.BufferList = tempList;
            }
            
            bool pending = Sock.SendAsync(SendEventArgs);
            if (!pending)
            {
                OnEventSendCompleted(null, SendEventArgs);
            }
        }
        catch (Exception e)
        {
            Close();
            Console.WriteLine("send error!! close socket. " + e.Message);
        }
    }

    
    
}
