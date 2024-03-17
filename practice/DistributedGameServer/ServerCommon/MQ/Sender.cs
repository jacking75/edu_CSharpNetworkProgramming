using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.MQ
{
    public class Sender
    {
        IConnection Connection = null;

        public void Init(string serverAddress)
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = serverAddress;

            Connection = new ConnectionFactory().CreateConnection(opts);
        }

        public void Destory()
        {
            Connection?.Dispose();
        }

        public void Send(string subject, byte[] payload, int offset, int count)
        {
            Connection.Publish(subject, payload, offset, count);
        }

        public void Send(string subject, byte[] payload)
        {
            Connection.Publish(subject, payload);
        }
    }
}
