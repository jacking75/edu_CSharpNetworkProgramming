using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace EchoServer;

class PacketProcess
{
    static Dictionary<UInt64, GameUser> UserList = new Dictionary<UInt64, GameUser>();


    bool IsStart = false;

    Thread LogicThread = null;

    FreeNet.IPacketDispatcher RefPacketDispatcher = null;

    FreeNet.NetworkService RefNetworkService = null;

    FreeNet.ServerOption ServerOpt;

    public PacketProcess(FreeNet.NetworkService netService)
    {
        RefNetworkService = netService;
        RefPacketDispatcher = netService.PacketDispatcher;

        ServerOpt = netService.ServerOpt;
    }

    /// <summary>
    /// 로직 스레드 시작.
    /// </summary>
    public void Start()
    {
        IsStart = true;

        LogicThread = new Thread(DoLogic);
        LogicThread.Start();
    }

    public void Stop()
    {
        IsStart = false;
        LogicThread.Join();
    }

    /// <summary>
    /// 로직 스레드. 
    /// </summary>
    void DoLogic()
    {
        // 반복문을 빠져나오도록 true 대신 bool 변수 사용하기
        while (IsStart)
        {
            // 메시지를 분배한다.
            var packetQueue = RefPacketDispatcher.DispatchAll();

            if (packetQueue.Count > 0)
            {
                while(packetQueue.TryDequeue(out var packet))
                {
                    // 패킷 처리를 한다.
                    OnMessage(packet);
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    void OnMessage(FreeNet.Packet packet)
    {
        // ex)
        PROTOCOL_ID protocol = (PROTOCOL_ID)packet.Id;
        //Console.WriteLine("------------------------------------------------------");
        //Console.WriteLine("protocol id " + protocol);
        switch (protocol)
        {
            case PROTOCOL_ID.ECHO:
                {
                    var requestPkt = new EchoPacket();
                    requestPkt.Decode(packet.BodyData);
                    Console.WriteLine(string.Format("text {0}", requestPkt.Data));

                    var responsePkt = new EchoPacket();
                    var packetData = responsePkt.ToPacket(PROTOCOL_ID.ECHO, packet.BodyData);
                    packet.Owner.Send(new ArraySegment<byte>(packetData, 0, packetData.Length));                                                
                }
                break;
            case PROTOCOL_ID.HEARTBEAT_UPDATE_NOTIFY:
                {
                    Console.WriteLine($"heartbeat: {DateTime.Now}");

                    packet.Owner.LatestHeartbeatTime = (UInt64)DateTime.Now.Ticks;
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
                Console.WriteLine($"SYS_NTF_CONNECTED: {session.UniqueId}");

                var user = new GameUser(session);
                UserList.Add(session.UniqueId, user);
                return true;

            // 이 처리는 꼭 해줘야 한다.
            case FreeNet.NetworkDefine.SYS_NTF_CLOSED:
                Console.WriteLine($"SYS_NTF_CLOSED: {session.UniqueId}");
                //RefNetworkService.OnSessionClosed(session); 
                UserList.Remove(session.UniqueId);
                return true;
                
            case FreeNet.NetworkDefine.SYS_START_HEARTBEAT:
                var notifyPkt = new HeartBeatStartNtfPacket();
                notifyPkt.IntervalSec = ServerOpt.HeartBeatIntervalSec;
                var pakcetData = notifyPkt.ToPacket();
                session.Send(new ArraySegment<byte>(pakcetData, 0, pakcetData.Length));
                return true;
        }                     

        return false;
    }
}
