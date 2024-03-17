using ServerCommon.MQ;

using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CenterServer
{
    public class PacketDistributor
    {
        static readonly ILogger<PacketDistributor> Logger = ServerCommon.LogManager.GetLogger<PacketDistributor>();

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        BufferBlock<(int,byte[])> MsgBuffer = new ();

        Dictionary<UInt16, Action<int, PacketHeaderInfo, byte[]>> PacketHandlerMap = new ();
        PKHandler.Handler Handler = new ();

        public Action<Int32, string, byte[], int> SendMqDataDelegate;


        public void CreateAndStart(ServerOption option)
        {
            Logger.ZLogInformation("[CreateAndStart] - begin");

            PKHandler.Handler.SendMqFunc = SendMqDataDelegate;
            PKHandler.Handler.ServerOpt = option;
            Handler.Init();

            RegistPacketHandler();

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();

            Logger.ZLogInformation("[CreateAndStart] - end");
        }

        public void Destory()
        {
            IsThreadRunning = false;
            MsgBuffer.Complete();
        }

        public void Distribute(int mqIndex, byte[] mqData) => MsgBuffer.Post((mqIndex,mqData));

        void Process()
        {
            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo();

            while (IsThreadRunning)
            {
                try
                {
                    var mqData = MsgBuffer.Receive();
                    
                    mqHeader.Read(mqData.Item2);

                    if (PacketHandlerMap.ContainsKey(mqHeader.ID))
                    {
                        PacketHandlerMap[mqHeader.ID](mqData.Item1, mqHeader, mqData.Item2);
                    }
                    else
                    {
                        Logger.ZLogError($"[Process] mqID: {mqHeader.ID}");
                    }
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => Console.WriteLine(ex.ToString()));
                }
            }
        }

        void RegistPacketHandler()
        {
            PacketHandlerMap.Add((UInt16)PacketID.ReqLobbyRoomMqInfo, Handler.RequestLobbyRoomMQInfo); 

        }
               
        
    }
}
