using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{
    public class MQReceiver
    {
        IConnection Connection = null;
        IAsyncSubscription Subscription = null;

        public Action<byte[]> ReceivedMQData;

        // qGroup가 공백이나 null이 아니면 Queue로 동작한다
        public void Init(string serverAddress, string subject, string qGroup)
        {
            
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = serverAddress;

            Connection = new ConnectionFactory().CreateConnection(opts);

            if (string.IsNullOrEmpty(qGroup))
            {
                Subscription = Connection.SubscribeAsync(subject, ReceiveHandler);
            }
            else
            {
                Subscription = Connection.SubscribeAsync(subject, qGroup);
                Subscription.MessageHandler += ReceiveHandler;
                Subscription.Start();
            }
        }

        private void ReceiveHandler(object m, MsgHandlerEventArgs ea)
        {
            //Console.WriteLine("Received: " + args.Message);
            ReceivedMQData(ea.Message.Data);
        }

        public void Destory()
        {
            Subscription?.Dispose();
            Connection?.Dispose();
        }
    }
}
