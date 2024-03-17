using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.MQ
{
    public class Receiver
    {
        int Index = 0;
        IConnection Connection = null;
        IAsyncSubscription Subscription = null;

        public Action<int, byte[]> ReceivedMQData;

        // qGroup가 공백이나 null이 아니면 Queue로 동작한다
        public void Init(int index, string serverAddress, string subject, string qGroup)
        {
            Index = index;

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
            ReceivedMQData(Index, ea.Message.Data);
        }

        public void Destory()
        {
            Subscription?.Dispose();
            Connection?.Dispose();
        }
    }
}
