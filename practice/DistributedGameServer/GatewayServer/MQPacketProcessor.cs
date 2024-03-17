using ServerCommon;

using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GatewayServer
{
    public class MQPacketProcessor
    {
        static readonly ILogger<MQPacketProcessor> Logger = LogManager.GetLogger<MQPacketProcessor>();

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        BufferBlock<byte[]> MsgBuffer = new BufferBlock<byte[]>();


        S2SPacket.Handler PacketHandler = new S2SPacket.Handler();

        Dictionary<UInt16, Action<ServerCommon.MQ.PacketHeaderInfo, byte[]>> HandlerFuncMap = new ();


        public Func<string, byte[], bool> S2CSendPacketFunc;
        public Action<Int16, byte[], Int32, Int32> SendMQToLobby;
        public Action<Int32, byte[], Int32, Int32> SendMQToGame;


        public void Init(ConnSession.Manager connSessionMgr,
                            MQSubjectManager mqSubjectMgr)
        {
            PacketHandler.Init(connSessionMgr, mqSubjectMgr);
            PacketHandler.SendNetworkFunc = S2CSendPacketFunc;
            PacketHandler.SendMQToLobby = SendMQToLobby;
            PacketHandler.SendMQToGame = SendMQToGame;

            RegistPacketHandler();

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }

        public void Destory()
        {
            if (IsThreadRunning)
            {
                IsThreadRunning = false;
                MsgBuffer.Complete();

                ProcessThread.Join();
            }
        }

        public void Distribute(byte[] mqData) => MsgBuffer.Post(mqData);

        void RegistPacketHandler()
        {
            HandlerFuncMap.Add(ServerCommon.MQ.PacketID.ResLobbyRoomMqInfo, PacketHandler.ResponseLoobyRoomMQInfo);
            HandlerFuncMap.Add(ServerCommon.MQ.PacketID.ResGatewayLogin, PacketHandler.ResponseLogIn);
            HandlerFuncMap.Add(ServerCommon.MQ.PacketID.ResLobbyEnter, PacketHandler.ResponseLobbyEnter);
            HandlerFuncMap.Add(ServerCommon.MQ.PacketID.ResLobbyLeave, PacketHandler.ResponseLobbyLeave);
            HandlerFuncMap.Add(ServerCommon.MQ.PacketID.ResLobbyRelay, PacketHandler.ResponseLobbyRelay);
        }


        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    Process_impl();
                }
                catch (Exception ex)
                {
                    if(IsThreadRunning)
                    {
                        Logger.ZLogError($"[Process] {ExceptionHelper.ExtractException(ex)}");
                    }                    
                }
            }
        }

        void Process_impl()
        {
            var mqData = MsgBuffer.Receive();
            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo();
            mqHeader.Read(mqData);

            var mqID = mqHeader.ID;

            if (HandlerFuncMap.ContainsKey(mqID))
            {
                HandlerFuncMap[mqID](mqHeader, mqData);
            }
            else
            {
                Logger.ZLogError($"[Process_impl] mqID: {mqID}");
            }
        }
    }
}
