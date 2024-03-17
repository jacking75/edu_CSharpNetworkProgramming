using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBServer
{
    public class PacketDistributor
    {       
        List<PacketProcessor> PacketProcessorList = new List<PacketProcessor>();

        int ThreadCount;

        int PacketNumber;

        Cache.Manager CacheMgr = new Cache.Manager();
        

        public void CreateAndStart(ServerOption option)
        {
            Program.GlobalLogger.LogInformation("[PacketDistributor.CreateAndStart] Start");

            ThreadCount = option.ThreadCount;
            PacketNumber = 0;
            PKHandler.Base.DBServerIndex = option.Index;

            Cache.Manager.Init(option.RedisAddress);


            for (int i = 0; i < ThreadCount; ++i)
            {
                var packetProcess = new PacketProcessor();
                packetProcess.CreateAndStart(CacheMgr);
                PacketProcessorList.Add(packetProcess);
            }

            Program.GlobalLogger.LogInformation("[PacketDistributor.CreateAndStart] End");
        }

        public void Destory()
        {
            PacketProcessorList.ForEach(preocess => preocess.Destory());
            PacketProcessorList.Clear();
        }

        public void Distribute(byte[] mqData)
        {

            var processor = DBNumberToPacketProcessor(PacketNumber++);
            if (processor != null)
            {
                processor.InsertMsg(mqData);
            }                  
        }

        PacketProcessor DBNumberToPacketProcessor(int number)
        {
            var nowPacketNum = number % ThreadCount;
            return PacketProcessorList[nowPacketNum];
        }
    }
}
