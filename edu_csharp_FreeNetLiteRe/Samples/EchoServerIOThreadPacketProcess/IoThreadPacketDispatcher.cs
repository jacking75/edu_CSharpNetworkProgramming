using System;
using System.Collections.Generic;

using FreeNet;
using System.Collections.Concurrent;

namespace EchoServerIOThreadPacketProcess
{
    class IoThreadPacketDispatcher : IPacketDispatcher
    {
        public static readonly UInt16 HEADERSIZE = 5;
        public static readonly UInt16 HEADER_PACKETID_POS = 2;

        FreeNet.NetworkService RefNetworkService = null;

        ConcurrentDictionary<UInt64, GameUser> UserList = new ConcurrentDictionary<UInt64, GameUser>();
                
        public Queue<Packet> DispatchAll() { return null; }

        public void OnReceive(Session session, byte[] buffer, int offset, int size)
        {
            var receiveBufferOffset = offset;
            var receiveBufferReaminSize = size;

            // 남은 데이터가 있다면 계속 반복한다.
            while (receiveBufferReaminSize >= HEADERSIZE)
            {
                (var packetSize, var packetId) = GetPacketSizeAndId(buffer, receiveBufferOffset);
                if (packetSize > receiveBufferReaminSize)
                {
                    break;
                }

                byte[] bodyData = null;
                var bodySize = packetSize - HEADERSIZE;
                if (bodySize > 0)
                {
                    bodyData = new byte[bodySize];
                }

                var packet = new Packet(packetId, bodyData);
                IncomingPacket(false, session, packet);

                receiveBufferOffset += packetSize;
                receiveBufferReaminSize -= packetSize;
            }

            //간결한 구현을 위해
            //receiveBufferReaminSize가 남는 상황은 고려하지 않는다!!!
        }

        (UInt16, UInt16) GetPacketSizeAndId(byte[] buffer, int offset)
        {
            var packetSize = FastBinaryRead.UInt16(buffer, offset);
            var packetId = FastBinaryRead.UInt16(buffer, offset + 2);
            return (packetSize, packetId);
        }


        public void IncomingPacket(bool IsSystem, Session user, Packet packet)
        {            
            //Console.WriteLine("------------------------------------------------------");
            //Console.WriteLine("protocol id " + protocol);

            if (IsSystem == false && (packet.Id <= NetworkDefine.SYS_NTF_MAX))
            {
                //TODO: 로그 남기기
                // 시스템만 보내어야할 패킷을 상대방이 보냈음. 해킹 의심
                return;
            }


            var protocol = (PROTOCOL_ID)packet.Id;

            switch (protocol)
            {
                case PROTOCOL_ID.ECHO_REQ:
                    {
                        var requestPkt = new EchoPacket();
                        requestPkt.Decode(packet.BodyData);
                        Console.WriteLine(string.Format("text {0}", requestPkt.Data));

                        var responsePkt = new EchoPacket();
                        var packetData = responsePkt.ToPacket(PROTOCOL_ID.ECHO_ACK, packet.BodyData);
                        packet.Owner.Send(new ArraySegment<byte>(packetData, 0, packetData.Length));
                    }
                    break;
                default:
                    {
                        if (OnSystemPacket(packet) == false)
                        {
                            Console.WriteLine("Unknown protocol id " + protocol);
                        }
                    }
                    break;
            }
        }
         
      
        bool OnSystemPacket(FreeNet.Packet packet)
        {
            var session = packet.Owner;

            // active close를 위한 코딩.
            //   서버에서 종료하라고 연락이 왔는지 체크한다.
            //   만약 종료신호가 맞다면 disconnect를 호출하여 받은쪽에서 먼저 종료 요청을 보낸다.
            switch (packet.Id)
            {
                // 이 처리는 꼭 해줘야 한다.
                case FreeNet.NetworkDefine.SYS_NTF_CONNECTED:
                    Console.WriteLine("SYS_NTF_CONNECTED : " + session.UniqueId);

                    var user = new GameUser(session);
                    UserList.TryAdd(session.UniqueId, user);
                    return true;

                // 이 처리는 꼭 해줘야 한다.
                case FreeNet.NetworkDefine.SYS_NTF_CLOSED:
                    Console.WriteLine("SYS_NTF_CLOSED : " + session.UniqueId);
                    //RefNetworkService.OnSessionClosed(session); 
                    UserList.TryRemove(session.UniqueId, out var temp);
                    return true;   
            }

            return false;
        }
        
    }
}
