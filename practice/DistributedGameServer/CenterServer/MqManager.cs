using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CenterServer
{
    class MqManager
    {
        static readonly ILogger<MqManager> Logger = ServerCommon.LogManager.GetLogger<MqManager>();

        List<ServerCommon.MQ.Receiver> ReceiverList = new();
        List<ServerCommon.MQ.Sender> SenderList = new();
                
        public void Init(Int32 serverIndex, List<string> mqServerAddressList, string subPreFix, Action<int, byte[]> receivedMQDataEvent)
        {
            int index = 0;
            var subName = subPreFix;

            foreach (var address in mqServerAddressList)
            {                
                var receiver = new ServerCommon.MQ.Receiver();
                receiver.Init(index, address, subName, null);
                receiver.ReceivedMQData = receivedMQDataEvent;
                ReceiverList.Add(receiver);


                var sender = new ServerCommon.MQ.Sender();
                sender.Init(address);
                SenderList.Add(sender);

                Logger.ZLogInformation($"[Init] MQ Index:{index}, Address:{address}, Sub:{subName}");

                ++index;
            }            
        }
                
        public void Destory()
        {
            foreach(var receiver in ReceiverList)
            {
                receiver.Destory();
            }

            foreach (var sender in SenderList)
            {
                sender.Destory();
            }
        }

        public void Send(int mqIndex, string subject, byte[] payload, int payloadLen)
        {
            SenderList[mqIndex].Send(subject, payload, 0, payloadLen);
        }
    }   
}
