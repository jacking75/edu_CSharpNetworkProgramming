using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using ServerCommon;

namespace DBServer
{
    class MqManager
    {
        MQReceiver Receiver = new MQReceiver(); 
        MQSender Sender = new MQSender();

        public void Init(string serverAddress, int dbServerIndex, Action<byte[]> receivedMQDataEvent)
        {
            var subName = $"db.req.{dbServerIndex}";
            Receiver.Init(serverAddress, subName, null);
            Receiver.ReceivedMQData = receivedMQDataEvent;
            Console.WriteLine($"[MQ Sub] Address:{serverAddress}, Sub:{subName}");
                        
            Sender.Init(serverAddress);
            Console.WriteLine($"[MQ Pub] Address:{serverAddress}");
        }
                
        public void Destory()
        {
            Receiver.Destory();            
            Sender.Destory();
        }

        public void SendMQ(string subject, byte[] payload)
        {
            Sender.Send(subject, payload);
        }     
    }   
}
