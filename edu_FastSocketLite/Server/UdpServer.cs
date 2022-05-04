using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FastSocketLite.Server
{
    /// <summary>
    /// upd server
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public sealed class UdpServer<TMessage> : IUdpServer<TMessage> where TMessage : class, Messaging.IMessage
    {
        private readonly int _port;
        private readonly int _messageBufferSize;

        private Socket _socket = null;
        private AsyncSendPool _pool = null;

        private readonly Protocol.IUdpProtocol<TMessage> _protocol = null;
        private readonly IUdpService<TMessage> _service = null;
        

        
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="service"></param>
        public UdpServer(int port, Protocol.IUdpProtocol<TMessage> protocol,
                                        IUdpService<TMessage> service)
            : this(port, 2048, protocol, service)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="protocol"></param>
        /// <param name="service"></param>
        /// <exception cref="ArgumentNullException">protocol is null.</exception>
        /// <exception cref="ArgumentNullException">service is null.</exception>
        public UdpServer(int port, int messageBufferSize,
                                        Protocol.IUdpProtocol<TMessage> protocol,
                                        IUdpService<TMessage> service)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            this._port = port;
            this._messageBufferSize = messageBufferSize;
            this._protocol = protocol;
            this._service = service;
        }
        

        
        private void BeginReceive(SocketAsyncEventArgs e)
        {
            if (!this._socket.ReceiveFromAsync(e))
                ThreadPool.QueueUserWorkItem(_ => this.ReceiveCompleted(this, e));
        }
        
        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                var session = new UdpSession(e.RemoteEndPoint, this);
                TMessage message = null;

                try
                {
                    message = this._protocol.Parse(new ArraySegment<byte>(e.Buffer, 0, e.BytesTransferred));
                }
                catch (Exception ex)
                {
                    SocketBase.Log.Trace.Error(ex.Message, ex);
                    this._service.OnError(session, ex);
                }

                if (message != null)
                {
                    this._service.OnReceived(session, message);
                }
            }

            //receive again
            this.BeginReceive(e);
        }
        

                
        public void Start()
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this._socket.Bind(new IPEndPoint(IPAddress.Any, this._port));
            this._socket.DontFragment = true;

            this._pool = new AsyncSendPool(this._messageBufferSize, this._socket);

            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
            e.Completed += this.ReceiveCompleted;
            this.BeginReceive(e);
        }
        
        public void Stop()
        {
            this._socket.Close();
            this._socket = null;
            this._pool = null;
        }
        
        public void SendTo(EndPoint endPoint, byte[] payload)
        {
            this._pool.SendAsync(endPoint, payload);
        }        
                
    } 
}