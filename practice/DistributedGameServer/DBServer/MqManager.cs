using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using ServerCommon;

namespace DBServer
{
    class MqManager
    {
        public static readonly ILogger<MqManager> Logger = LogManager.GetLogger<MqManager>();

        List<ServerCommon.MQ.Receiver> ReceiverList = new();
        List<ServerCommon.MQ.Sender> SenderList = new();

        public void Init(ServerOption serverOpt, Action<int, byte[]> receivedMQDataEvent)
        {
            var index = 0;
            var subName = ServerCommon.MQ.SubjectManager.ToDBServer();

            foreach (var address in serverOpt.MQServerAddressList)
            {
                var receiver = new ServerCommon.MQ.Receiver();
                receiver.Init(index, address, subName, serverOpt.MQSubQueueName);
                receiver.ReceivedMQData = receivedMQDataEvent;
                ReceiverList.Add(receiver);


                var sender = new ServerCommon.MQ.Sender();
                sender.Init(address);
                SenderList.Add(sender);

                Logger.ZLogInformation(new EventId(LogEventID.DBProgramInit), $"[Init] Index: {index}, MQAddress:{address}, Sub:{subName}");

                ++index;
            }
        }

        public void Destory()
        {
            foreach (var receiver in ReceiverList)
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
