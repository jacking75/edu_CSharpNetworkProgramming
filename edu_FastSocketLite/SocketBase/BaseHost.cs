using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FastSocketLite.SocketBase
{
    /// <summary>
    /// base host
    /// </summary>
    public abstract class BaseHost : IHost
    {
        private long _connectionID = 1_000L;
        private readonly ConnectionCollection _listConnections = new ConnectionCollection();
        internal readonly SocketAsyncEventArgsPool _saePool = null;
        

        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <exception cref="ArgumentOutOfRangeException">socketBufferSize</exception>
        /// <exception cref="ArgumentOutOfRangeException">messageBufferSize</exception>
        protected BaseHost(int socketBufferSize, int messageBufferSize)
        {
            if (socketBufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("socketBufferSize");
            }

            if (messageBufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("messageBufferSize");
            }

            this.SocketBufferSize = socketBufferSize;
            this.MessageBufferSize = messageBufferSize;
            this._saePool = new SocketAsyncEventArgsPool(messageBufferSize);
        }
        

        public int SocketBufferSize
        {
            get;
            private set;
        }
        
        public int MessageBufferSize
        {
            get;
            private set;
        }

        /// <summary>
        /// create new <see cref="IConnection"/>
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">socket is null</exception>
        public virtual IConnection NewConnection(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            socket.NoDelay = true;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            socket.ReceiveBufferSize = this.SocketBufferSize;
            socket.SendBufferSize = this.SocketBufferSize;

            return new DefaultConnection(this.NextConnectionID(), socket, this);
        }
        
        public IConnection GetConnectionByID(long connectionID)
        {
            return this._listConnections.Get(connectionID);
        }
        
        public IConnection[] ListAllConnection()
        {
            return this._listConnections.ToArray();
        }
        
        public int CountConnection()
        {
            return this._listConnections.Count();
        }

        public virtual void Start()
        {
        }
        
        public virtual void Stop()
        {
            this._listConnections.DisconnectAll();
        }
        

        /// <summary>
        /// 새로운 Connection ID 생성
        /// </summary>
        /// <returns></returns>
        protected long NextConnectionID()
        {
            return Interlocked.Increment(ref this._connectionID);
        }
        
        /// <summary>
        /// register connection
        /// </summary>
        /// <param name="connection"></param>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        protected void RegisterConnection(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.Active)
            {
                this._listConnections.Add(connection);
                this.OnConnected(connection);
            }
        }

        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        virtual public void OnConnected(IConnection connection)
        {
            Log.Trace.Debug(string.Concat("socket connected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? "unknow" : connection.RemoteEndPoint.ToString(),
                ", local endPoint:", connection.LocalEndPoint == null ? "unknow" : connection.LocalEndPoint.ToString()));
        }

        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        virtual public void OnStartSending(IConnection connection, Packet packet)
        {
        }

        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        virtual public void OnSendCallback(IConnection connection, Packet packet, bool isSuccess)
        {
        }

        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        virtual public void OnMessageReceived(IConnection connection, MessageReceivedEventArgs e)
        {
        }

        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        virtual public void OnDisconnected(IConnection connection, Exception ex)
        {
            this._listConnections.Remove(connection.ConnectionID);
            Log.Trace.Debug(string.Concat("socket disconnected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? "unknow" : connection.RemoteEndPoint.ToString(),
                ", local endPoint:", connection.LocalEndPoint == null ? "unknow" : connection.LocalEndPoint.ToString(),
                ex == null ? string.Empty : string.Concat(", reason is: ", ex.ToString())));
        }

        /// <summary>
        /// OnError
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        virtual public void OnConnectionError(IConnection connection, Exception ex)
        {
            Log.Trace.Error(ex.Message, ex);
        }
        
    }
}