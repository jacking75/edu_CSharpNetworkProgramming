using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LobbyServer
{
    public class PacketDistributor
    {
        static readonly ILogger<PacketDistributor> Logger = ServerCommon.LogManager.GetLogger<PacketDistributor>();

        List<PacketProcessor> PacketProcessorList = new List<PacketProcessor>();
                
        LobbyManager LobbyMgrRef;

    
        public void CreateAndStart(ServerOption serverOpt, LobbyManager lobbyMgr)
        {
            Logger.ZLogInformation("[CreateAndStart] - begin");

            LobbyMgrRef = lobbyMgr;
            var lobbyThreadCount = serverOpt.ThreadCount;
                        
            for (int i = 0; i < lobbyThreadCount; ++i)
            {
                var packetProcess = new PacketProcessor();
                packetProcess.CreateAndStart(serverOpt.ServerIndex, 
                                            LobbyMgrRef.GetLobbyList(i));

                PacketProcessorList.Add(packetProcess);
            }

            Logger.ZLogInformation("[CreateAndStart] - end");
        }

        public void Destory()
        {            
            PacketProcessorList.ForEach(preocess => preocess.Destory());
            PacketProcessorList.Clear();
        }

        public void Distribute(int mqIndex, byte[] mqData)
        {
            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo();
            mqHeader.Read(mqData);

            var processor = LobbyNumberToPacketProcessor(mqHeader.LobbyNumber);
            if (processor != null)
            {
                processor.InsertMsg(mqIndex,mqData);
            }
            else
            {
                Logger.ZLogError($"[Distribute] Invalid Lobby Num:{mqHeader.LobbyNumber}");
            }            
        }

        PacketProcessor LobbyNumberToPacketProcessor(Int16 number)
        {
            foreach(var processor in PacketProcessorList)
            {
                if(processor.관리중인_Lobby(number))
                {
                    return processor;
                }
            }
            return null;
        }

        
    }
}
