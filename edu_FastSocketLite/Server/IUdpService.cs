using System;

namespace FastSocketLite.Server
{
    /// <summary>
    /// udp service interface.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IUdpService<TMessage> where TMessage : class, Messaging.IMessage
    {
        void OnReceived(UdpSession session, TMessage message);
        
        void OnError(UdpSession session, Exception ex);
    }
}