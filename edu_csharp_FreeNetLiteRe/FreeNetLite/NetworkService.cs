using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace FreeNetLite;

public class NetworkService
{
    SocketAsyncEventArgsPool ReceiveEventArgsPool;
    SocketAsyncEventArgsPool SendEventArgsPool;

    Listener ClientListener = new();

    public IPacketDispatcher PacketDispatcher { get; private set; }
        
    public ServerOption ServerOpt { get; private set; }

    Int64 ConnectdSessionCount;

    UInt64 SequenceId = 0;
        
    public NetworkService(ServerOption serverOption, IPacketDispatcher packetDispatcher = null)
    {
        ServerOpt = serverOption;        
        PacketDispatcher = packetDispatcher;        
    }


    public void Initialize()
    {
        CreateEventArgsPool(ServerOpt.MaxConnectionCount, ServerOpt.ReceiveBufferSize);
    }

    public void Stop()
    {
        ClientListener.Stop();
    }

    public void Listen(string host, int port, int backlog, bool isNonDelay)
    {
        ClientListener = new Listener();
        ClientListener.OnNewClientCallback += OnNewClient;
        ClientListener.Start(host, port, backlog, isNonDelay);
    }

    public Int64 CurrentConnectdSessionCount()
    {
        Interlocked.Read(ref ConnectdSessionCount);
        return ConnectdSessionCount;
    }


    // 스레드 세이프 하지 않다
    UInt64 MakeSequenceIdForSession() { return ++SequenceId; }

    void CreateEventArgsPool(int maxConnectionCount, int receiveBufferSize)
    {
        const int pre_alloc_count = 1;
        int argsCount = maxConnectionCount * pre_alloc_count;
        int argsBufferSize = receiveBufferSize;

        ReceiveEventArgsPool = new SocketAsyncEventArgsPool();
        ReceiveEventArgsPool.Init(argsCount, argsBufferSize);

        SendEventArgsPool = new SocketAsyncEventArgsPool();

        SocketAsyncEventArgs arg;

        for (int i = 0; i < maxConnectionCount; i++)
        {
            // receive pool
            {
                arg = new SocketAsyncEventArgs();
                arg.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
                arg.UserToken = null;
                         
                // add SocketAsyncEventArg to the pool
                ReceiveEventArgsPool.Allocate(arg);
            }


            // send pool
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                arg = new SocketAsyncEventArgs();
                arg.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
                arg.UserToken = null;

                // send버퍼는 보낼때 설정한다. SetBuffer가 아닌 BufferList를 사용.
                arg.SetBuffer(null, 0, 0);
                
                // add SocketAsyncEventArg to the pool
                SendEventArgsPool.Push(arg);
            }
        }
    }

    void OnNewClient(bool isAccepted, Socket client_socket)
    {
        Console.WriteLine("OnNewClient [[[");

        // UserToken은 매번 새로 생성하여 깨끗한 인스턴스로 넣어준다.
        var uniqueId = MakeSequenceIdForSession();
        var user_token = new Session(isAccepted, uniqueId, PacketDispatcher, ServerOpt);
        user_token.OnEventSessionClosed += OnSessionClosed;
        user_token.OnEventSendCompleted += OnSendCompleted;
                
        // 플에서 하나 꺼내와 사용한다.
        SocketAsyncEventArgs receive_args = this.ReceiveEventArgsPool.Pop();
        SocketAsyncEventArgs send_args = this.SendEventArgsPool.Pop();

        receive_args.UserToken = user_token;
        send_args.UserToken = user_token;


        user_token.OnConnected();

        BeginReceive(client_socket, receive_args, send_args);

        Console.WriteLine("OnNewClient ]]]");
    }

    void BeginReceive(Socket socket, SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
    {
        // receive_args, send_args 아무곳에서나 꺼내와도 된다. 둘다 동일한 CUserToken을 물고 있다.
        var session = receive_args.UserToken as Session;
        if (session == null || session.IsConnected() == false)
        {
            return;
        }

        session.SetEventArgs(receive_args, send_args);
        session.Sock = socket;

        bool pending = socket.ReceiveAsync(receive_args);
        if (!pending)
        {
            OnReceive(receive_args);
        }
    }

    void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation == SocketAsyncOperation.Receive)
        {
            OnReceive(e);
            return;
        }
    }

    void OnSendCompleted(object sender, SocketAsyncEventArgs e)
    {
        var session = e.UserToken as Session;
        if (session == null || session.IsConnected() == false)
        {
            return;
        }

        try
        {
            if(session.SendCompleted(e) == false)
            {
                session.Close();
            }
        }
        catch (Exception)
        {
            session.Close();
        }
    }

    void OnReceive(SocketAsyncEventArgs e)
    {
        var session = e.UserToken as Session;
        if (session == null || session.IsConnected() == false)
        {
            return;
        }
 
        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            session.OnReceive(e.Buffer, e.Offset, e.BytesTransferred);

            
            if (session.Sock.ReceiveAsync(e) == false)
            {
                OnReceive(e);
            }
        }
        else
        {
            session.Close();
        }
    }

    void OnSessionClosed(Session session)
    {
        ReceiveEventArgsPool.Push(session.ReceiveEventArgs);        
        SendEventArgsPool.Push(session.SendEventArgs);
        
        session.SetEventArgs(null, null);
    }




}
