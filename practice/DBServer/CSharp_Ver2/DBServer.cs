using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

using ServerCommon;

namespace DBServer
{
    class DBServer
    {
        public static ServerOption ServerOption; 
        MqManager MQMgr = new MqManager();

        PacketDistributor Distributor = new PacketDistributor();

        public ErrorCode Start(ServerOption option)
        {
            ServerOption = option;
            
            MQMgr.Init(ServerOption.MQServerAddress, ServerOption.Index, ReceivedMQData);

            Distributor.CreateAndStart(option);
            
            PacketProcessor.MQSendFunc = SendMqData;
            
            return ErrorCode.None;
        }

      
        public void Stop()
        {
            Program.GlobalLogger.LogInformation("Server Stop <<<<");
            
            MQMgr.Destory();

            Distributor.Destory();

            Program.GlobalLogger.LogInformation("Server Stop >>>");
        }


        void ReceivedMQData(byte[] data)
        {
            Distributor.Distribute(data);
        }

        void SendMqData(int tagetServerIndex, byte[] data)
        {
            MQMgr.SendMQ($"db.res.{tagetServerIndex}", data);
        }


    } 
}
