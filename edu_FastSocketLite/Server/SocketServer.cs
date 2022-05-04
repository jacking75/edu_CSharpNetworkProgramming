using System;
using System.Net;

namespace FastSocketLite.Server
{
    /// <summary>
    /// socket server.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class SocketServer<TMessage> : SocketBase.BaseHost where TMessage : class, Messaging.IMessage
    {        
        private readonly SocketListener _listener = null;
        private readonly ISocketService<TMessage> _socketService = null;
        private readonly Protocol.IProtocol<TMessage> _protocol = null;
        private readonly int _maxMessageSize;
        private readonly int _maxConnections;
        

        
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="socketService"></param>
        /// <param name="protocol"></param>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="maxConnections"></param>
        /// <exception cref="ArgumentNullException">socketService is null.</exception>
        /// <exception cref="ArgumentNullException">protocol is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxMessageSize</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxConnections</exception>
        public SocketServer(int port,
            ISocketService<TMessage> socketService,
            Protocol.IProtocol<TMessage> protocol,
            int socketBufferSize,
            int messageBufferSize,
            int maxMessageSize,
            int maxConnections)
            : base(socketBufferSize, messageBufferSize)
        {
            if (socketService == null)
            {
                throw new ArgumentNullException("socketService");
            }

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (maxMessageSize < 1)
            {
                throw new ArgumentOutOfRangeException("maxMessageSize");
            }

            if (maxConnections < 1)
            {
                throw new ArgumentOutOfRangeException("maxConnections");
            }

            this._socketService = socketService;
            this._protocol = protocol;
            this._maxMessageSize = maxMessageSize;
            this._maxConnections = maxConnections;

            this._listener = new SocketListener(new IPEndPoint(IPAddress.Any, port), this);
            this._listener.Accepted += this.OnAccepted;
        }
        

        /// <summary>
        /// socket accepted handler
        /// </summary>
        /// <param name="connection"></param>
        private void OnAccepted(SocketBase.IConnection connection)
        {
            if (base.CountConnection() < this._maxConnections)
            {
                base.RegisterConnection(connection);
                return;
            }

            SocketBase.Log.Trace.Info("too many connections.");
            connection.BeginDisconnect();
        }
        

        
        public override void Start()
        {
            base.Start();
            this._listener.Start();
        }

        public override void Stop()
        {
            this._listener.Stop();
            base.Stop();
        }
        
        override public void OnConnected(SocketBase.IConnection connection)
        {
            base.OnConnected(connection);
            this._socketService.OnConnected(connection);
        }
                
        override public void OnSendCallback(SocketBase.IConnection connection,
            SocketBase.Packet packet, bool isSuccess)
        {
            base.OnSendCallback(connection, packet, isSuccess);
            this._socketService.OnSendCallback(connection, packet, isSuccess);
        }
                
        override public void OnMessageReceived(SocketBase.IConnection connection,
            SocketBase.MessageReceivedEventArgs e)
        {
            base.OnMessageReceived(connection, e);

            int readlength;
            TMessage message = null;
            try
            {
                message = this._protocol.Parse(connection, e.Buffer, this._maxMessageSize, out readlength);
            }
            catch (Exception ex)
            {
                this.OnConnectionError(connection, ex);
                connection.BeginDisconnect(ex);
                e.SetReadlength(e.Buffer.Count);
                return;
            }

            if (message != null)
            {
                this._socketService.OnReceived(connection, message);
            }

            e.SetReadlength(readlength);
        }

        override public void OnDisconnected(SocketBase.IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            this._socketService.OnDisconnected(connection, ex);
        }

        override public void OnConnectionError(SocketBase.IConnection connection, Exception ex)
        {
            base.OnConnectionError(connection, ex);
            this._socketService.OnException(connection, ex);
        }
        
    }
}