using System;

namespace FastSocketLite.Server
{
    /// <summary>
    /// abstract socket service interface.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class AbsSocketService<TMessage> : ISocketService<TMessage>
        where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// 이 메소드는 소켓 연결이 설정 될 때 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        public virtual void OnConnected(SocketBase.IConnection connection)
        {
        }
        
        public virtual void OnSendCallback(SocketBase.IConnection connection, SocketBase.Packet packet, bool isSuccess)
        {
        }

        /// <summary>
        /// 이 메소드는 새로운 클라이언트 메시지가 수신 될 때 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        public virtual void OnReceived(SocketBase.IConnection connection, TMessage message)
        {
        }
        /// <summary>
        /// 이 메소드는 소켓 연결이 끊어지면 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public virtual void OnDisconnected(SocketBase.IConnection connection, Exception ex)
        {
        }

        /// <summary>
        /// 이 메소드는 예외가 발생할 때 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        public virtual void OnException(SocketBase.IConnection connection, Exception ex)
        {
        }
    }
}