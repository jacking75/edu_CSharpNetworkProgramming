using ServerCommon;
using ServerCommon.MQ;

using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

namespace LobbyServer
{
    class PacketProcessor
    {
        static readonly ILogger<PacketProcessor> Logger = LogManager.GetLogger<PacketProcessor>();

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        BufferBlock<(int,byte[])> MsgBuffer = new ();

        Tuple<int,int> LobbyNumberRange = new Tuple<int, int>(-1, -1);
        List<Lobby> LobbyList = new List<Lobby>();

        Dictionary<UInt16, Action<int, PacketHeaderInfo, byte[]>> PacketHandlerMap = new ();
        PKHandler.Handler PacketHandler = new();

        public static Action<int, string, byte[], int> MQSendFunc;


        public void CreateAndStart(UInt16 serverIndex, List<Lobby> lobbyList)
        {
            LobbyList = lobbyList;

            var minLobbyNum = LobbyList[0].Number;
            var maxLobbyNum = LobbyList[0].Number + LobbyList.Count() - 1;
            LobbyNumberRange = new Tuple<int, int>(minLobbyNum, maxLobbyNum);


            PacketHandler.Init(serverIndex, (Int16)minLobbyNum, LobbyList);

            RegistPacketHandler();


            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }
        
        public void Destory()
        {
            IsThreadRunning = false;
            MsgBuffer.Complete();
        }

        public bool 관리중인_Lobby(int lobbyNumber)
        {
            //InRange의 min, max도 포함된다.
            return lobbyNumber.InRange(LobbyNumberRange.Item1, LobbyNumberRange.Item2);
        }

        public void InsertMsg(int mqIndex, byte[] mqData) => MsgBuffer.Post((mqIndex,mqData));
          
        
        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    var (mqIndex, mqData) = MsgBuffer.Receive();
                    var mqHeader = new PacketHeaderInfo();
                    mqHeader.Read(mqData);

                    var mqID = mqHeader.ID;
                    if(mqID == PacketID.ReqLobbyRelay)
                    {
                        mqID = PacketIDFromRelqyPacket(mqData);
                    }

                    if (PacketHandlerMap.ContainsKey(mqID))
                    {
                        PacketHandlerMap[mqID](mqIndex, mqHeader, mqData);
                    }
                    else
                    {
                        Logger.ZLogError($"[Process] Invalid mqID: {mqID}");                
                    }
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => Console.WriteLine(ex.ToString()));
                }
            }
        }

        UInt16 PacketIDFromRelqyPacket(byte[] relayPacket)
        {
            return CSCommon.MsgPackPacketHeaderInfo.ReadPacketID(relayPacket, PacketHeaderInfo.HeadSize);
        }

        void RegistPacketHandler()
        {
            PacketHandlerMap.Add(PacketID.ReqLobbyEnter, PacketHandler.RequestLobbyEnter);
            PacketHandlerMap.Add(PacketID.ReqLobbyLeave, PacketHandler.RequestLobbyLeave);
            PacketHandlerMap.Add(CSCommon.PacketID.ReqLobbyChat, PacketHandler.RequestLobbyChat);
        }          

    }
}
