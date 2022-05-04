using System;

namespace FastSocketLite.Server
{
    /// <summary>
    /// udp service
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class AbsUdpService<TMessage> : IUdpService<TMessage>
        where TMessage : class, Messaging.IMessage
    {
        public virtual void OnReceived(UdpSession session, TMessage message)
        {
        }
        
        public virtual void OnError(UdpSession session, Exception ex)
        {
        }
    }
}