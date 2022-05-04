using System;

namespace FastSocketLite.Server
{
    /// <summary>
    /// socket service interface.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface ISocketService<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// 이 메소드는 소켓 연결이 설정 될 때 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        void OnConnected(SocketBase.IConnection connection);

        void OnSendCallback(SocketBase.IConnection connection, SocketBase.Packet packet, bool isSuccess);

        /// <summary>
        ///이 메소드는 새로운 클라이언트 메시지가 수신 될 때 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        void OnReceived(SocketBase.IConnection connection, TMessage message);

        /// <summary>
        /// 이 메소드는 소켓 연결이 끊어지면 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        void OnDisconnected(SocketBase.IConnection connection, Exception ex);

        /// <summary>
        /// 이 메소드는 예외가 발생할 때 호출된다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        void OnException(SocketBase.IConnection connection, Exception ex);
    }
}