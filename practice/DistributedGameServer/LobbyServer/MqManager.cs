using Microsoft.Extensions.Logging;
using ZLogger;
using ServerCommon;
using ServerCommon.MQ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace LobbyServer
{
    class MqManager
    {
        static readonly ILogger<MqManager> Logger = LogManager.GetLogger<MqManager>();

        List<Receiver> ReceiverList = new ();
        List<Sender> SenderList = new ();
                
        public void Init(ServerOption serverOpt, Action<int, byte[]> receivedMQDataFunc)
        {
            int index = 0;
            var subName = SubjectManager.ToLobbyServer(serverOpt.ServerIndex);

            foreach (var address in serverOpt.MQServerAddressList)
            {
                var receiver = new Receiver();
                receiver.Init(index, address, subName, null);
                receiver.ReceivedMQData = receivedMQDataFunc;
                ReceiverList.Add(receiver);


                var sender = new Sender();
                sender.Init(address);
                SenderList.Add(sender);

                Logger.ZLogInformation(new EventId(LogEventID.DBProgramInit), $"Index: {index}, MQAddress:{address}, Sub:{subName}");

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
