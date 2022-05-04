using System.Net;

namespace FastSocketLite.Server
{
    /// <summary>
    /// upd server interface
    /// </summary>
    public interface IUdpServer
    {
        void Start();
        
        void Stop();
        
        void SendTo(EndPoint endPoint, byte[] payload);
    }

    /// <summary>
    /// upd server interface
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IUdpServer<TMessage> : IUdpServer  where TMessage : class, Messaging.IMessage
    {
    }
}