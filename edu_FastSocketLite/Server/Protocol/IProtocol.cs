using FastSocketLite.SocketBase;
using System;

namespace FastSocketLite.Server.Protocol
{
    /// <summary>
    /// tcp 프로토콜 인터페이스
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IProtocol<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// parse protocol message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        TMessage Parse(IConnection connection, ArraySegment<byte> buffer,
            int maxMessageSize, out int readlength);
    }
}