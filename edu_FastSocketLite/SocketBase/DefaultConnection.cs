using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;

namespace FastSocketLite.SocketBase
{
    internal class DefaultConnection : IConnection
    {
        private int _active = 1;
        private DateTime _latestActiveTime = Utils.Date.UtcNow;
        private readonly int _messageBufferSize;
        private readonly BaseHost _host = null;

        private readonly Socket _socket = null;

        private SocketAsyncEventArgs _saeSend = null;
        private Packet _currSendingPacket = null;
        private readonly PacketQueue _packetQueue = null;

        private SocketAsyncEventArgs _saeReceive = null;
        private MemoryStream _tsStream = null;
        private int _isReceiving = 0;


        /// <summary>
        /// new
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="socket"></param>
        /// <param name="host"></param>
        /// <exception cref="ArgumentNullException">socket is null</exception>
        /// <exception cref="ArgumentNullException">host is null</exception>
        public DefaultConnection(long connectionID, Socket socket, BaseHost host)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            if (host == null)
            {
                throw new ArgumentNullException("host");
            }

            this.ConnectionID = connectionID;
            this._socket = socket;
            this._messageBufferSize = host.MessageBufferSize;
            this._host = host;

            try
            {
                this.LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
                this.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            }
            catch (Exception ex)
            {
                Log.Trace.Error("get socket endPoint error.", ex);
            }

            //init send
            this._saeSend = host._saePool.Acquire();
            this._saeSend.Completed += this.SendAsyncCompleted;
            this._packetQueue = new PacketQueue(this.SendPacketInternal);

            //init receive
            this._saeReceive = host._saePool.Acquire();
            this._saeReceive.Completed += this.ReceiveAsyncCompleted;
        }


        public event DisconnectedHandler Disconnected;

        /// <summary>
        /// return the connection is active.
        /// </summary>
        public bool Active
        {
            get { return Thread.VolatileRead(ref this._active) == 1; }
        }

        /// <summary>
        /// get the connection latest active time.
        /// </summary>
        public DateTime LatestActiveTime
        {
            get { return this._latestActiveTime; }
        }

        /// <summary>
        /// get the connection id.
        /// </summary>
        public long ConnectionID { get; private set; }

        public IPEndPoint LocalEndPoint { get; private set; }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public object UserData { get; set; }

        public void BeginSend(Packet packet)
        {
            if (!this._packetQueue.TrySend(packet))
            {
                this.OnSendCallback(packet, false);
            }
        }

        public void BeginReceive()
        {
            if (Interlocked.CompareExchange(ref this._isReceiving, 1, 0) == 0)
            {
                this.ReceiveInternal();
            }
        }

        public void BeginDisconnect(Exception ex = null)
        {
            if (Interlocked.CompareExchange(ref this._active, 0, 1) == 1)
            {
                this.DisconnectInternal(ex);
            }
        }


        /// <summary>
        /// free send queue
        /// </summary>
        private void FreeSendQueue()
        {
            var result = this._packetQueue.Close();
            if (result.BeforeState == PacketQueue.CLOSED)
            {
                return;
            }

            if (result.Packets != null)
            {
                foreach (var p in result.Packets)
                {
                    this.OnSendCallback(p, false);
                }
            }

            if (result.BeforeState == PacketQueue.IDLE)
            {
                this.FreeSend();
            }
        }

        /// <summary>
        /// free for send.
        /// </summary>
        private void FreeSend()
        {
            this._currSendingPacket = null;
            this._saeSend.Completed -= this.SendAsyncCompleted;
            this._host._saePool.Release(this._saeSend);
            this._saeSend = null;
        }

        /// <summary>
        /// free fo receive.
        /// </summary>
        private void FreeReceive()
        {
            this._saeReceive.Completed -= this.ReceiveAsyncCompleted;
            this._host._saePool.Release(this._saeReceive);
            this._saeReceive = null;
            if (this._tsStream != null)
            {
                this._tsStream.Close();
                this._tsStream = null;
            }
        }


        /// <summary>
        /// fire StartSending
        /// </summary>
        /// <param name="packet"></param>
        public void OnStartSending(Packet packet)
        {
            this._host.OnStartSending(this, packet);
        }

        /// <summary>
        /// fire SendCallback
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        private void OnSendCallback(Packet packet, bool isSuccess)
        {
            if (isSuccess) this._latestActiveTime = Utils.Date.UtcNow;
            else packet.SentSize = 0;

            this._host.OnSendCallback(this, packet, isSuccess);
        }

        /// <summary>
        /// fire MessageReceived
        /// </summary>
        /// <param name="e"></param>
        private void OnMessageReceived(MessageReceivedEventArgs e)
        {
            this._latestActiveTime = Utils.Date.UtcNow;
            this._host.OnMessageReceived(this, e);
        }

        /// <summary>
        /// fire Disconnected
        /// </summary>
        private void OnDisconnected(Exception ex)
        {
            if (this.Disconnected != null)
            {
                this.Disconnected(this, ex);
            }

            this._host.OnDisconnected(this, ex);
        }

        /// <summary>
        /// fire Error
        /// </summary>
        /// <param name="ex"></param>
        private void OnError(Exception ex)
        {
            this._host.OnConnectionError(this, ex);
        }


        #region Send
        /// <summary>
        /// internal send packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <exception cref="ArgumentNullException">packet is null</exception>
        private void SendPacketInternal(Packet packet)
        {
            this._currSendingPacket = packet;
            this.OnStartSending(packet);
            this.SendPacketInternal(this._saeSend);
        }
        /// <summary>
        /// internal send packet.
        /// </summary>
        /// <param name="e"></param>
        private void SendPacketInternal(SocketAsyncEventArgs e)
        {
            var packet = this._currSendingPacket;

            //messageBufferSize 크기로 블럭 전송
            var length = Math.Min(packet.Payload.Length - packet.SentSize, this._messageBufferSize);
            var completedAsync = true;
            try
            {
                //copy data to send buffer
                Buffer.BlockCopy(packet.Payload, packet.SentSize, e.Buffer, 0, length);
                e.SetBuffer(0, length);
                completedAsync = this._socket.SendAsync(e);
            }
            catch (Exception ex)
            {
                this.BeginDisconnect(ex);
                this.FreeSend();
                this.OnSendCallback(packet, false);
                this.OnError(ex);
            }

            if (!completedAsync)
                ThreadPool.QueueUserWorkItem(_ => this.SendAsyncCompleted(this, e));
        }

        /// <summary>
        /// async send callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            var packet = this._currSendingPacket;

            //send error!
            if (e.SocketError != SocketError.Success)
            {
                this.BeginDisconnect(new SocketException((int)e.SocketError));
                this.FreeSend();
                this.OnSendCallback(packet, false);
                return;
            }

            packet.SentSize += e.BytesTransferred;

            if (e.Offset + e.BytesTransferred < e.Count)
            {
                //continue to send until all bytes are sent!
                var completedAsync = true;
                try
                {
                    e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred - e.Offset);
                    completedAsync = this._socket.SendAsync(e);
                }
                catch (Exception ex)
                {
                    this.BeginDisconnect(ex);
                    this.FreeSend();
                    this.OnSendCallback(packet, false);
                    this.OnError(ex);
                }

                if (!completedAsync)
                    ThreadPool.QueueUserWorkItem(_ => this.SendAsyncCompleted(sender, e));
            }
            else
            {
                if (packet.IsSent())
                {
                    this._currSendingPacket = null;
                    this.OnSendCallback(packet, true);

                    //try send next packet
                    if (!this._packetQueue.TrySendNext())
                    {
                        this.FreeSend();
                    }
                }
                else
                {
                    this.SendPacketInternal(e);//continue send this packet
                }
            }
        }
        #endregion

        /// <summary>
        /// receive
        /// </summary>
        private void ReceiveInternal()
        {
            bool completed = true;
            try { completed = this._socket.ReceiveAsync(this._saeReceive); }
            catch (Exception ex)
            {
                this.BeginDisconnect(ex);
                this.FreeReceive();
                this.OnError(ex);
            }

            if (!completed)
                ThreadPool.QueueUserWorkItem(_ => this.ReceiveAsyncCompleted(this, this._saeReceive));
        }

        /// <summary>
        /// async receive callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                this.BeginDisconnect(new SocketException((int)e.SocketError));
                this.FreeReceive();
                return;
            }

            if (e.BytesTransferred < 1)
            {
                this.BeginDisconnect();
                this.FreeReceive();
                return;
            }

            ArraySegment<byte> buffer;
            var ts = this._tsStream;
            if (ts == null || ts.Length == 0)
                buffer = new ArraySegment<byte>(e.Buffer, 0, e.BytesTransferred);
            else
            {
                ts.Write(e.Buffer, 0, e.BytesTransferred);
                buffer = new ArraySegment<byte>(ts.GetBuffer(), 0, (int)ts.Length);
            }

            this.OnMessageReceived(new MessageReceivedEventArgs(buffer, this.MessageProcessCallback));
        }

        /// <summary>
        /// message process callback
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="readlength"></param>
        /// <exception cref="ArgumentOutOfRangeException">readlength less than 0 or greater than payload.Count.</exception>
        private void MessageProcessCallback(ArraySegment<byte> payload, int readlength)
        {
            if (readlength < 0 || readlength > payload.Count)
                throw new ArgumentOutOfRangeException("readlength", "readlength less than 0 or greater than payload.Count.");

            var ts = this._tsStream;
            if (readlength == 0)
            {
                if (ts == null)
                {
                    this._tsStream = ts = new MemoryStream(this._messageBufferSize);
                }
                else
                {
                    ts.SetLength(0);
                }

                ts.Write(payload.Array, payload.Offset, payload.Count);
                this.ReceiveInternal();
                return;
            }

            if (readlength == payload.Count)
            {
                if (ts != null)
                {
                    ts.SetLength(0);
                }

                this.ReceiveInternal();
                return;
            }

            this.OnMessageReceived(new MessageReceivedEventArgs(
                new ArraySegment<byte>(payload.Array, payload.Offset + readlength, payload.Count - readlength),
                this.MessageProcessCallback));
        }


        /// <summary>
        /// disconnect
        /// </summary>
        /// <param name="reason"></param>
        private void DisconnectInternal(Exception reason)
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += this.DisconnectAsyncCompleted;
            e.UserToken = reason;

            var completedAsync = true;
            try
            {
                this._socket.Shutdown(SocketShutdown.Both);
                completedAsync = this._socket.DisconnectAsync(e);
            }
            catch (Exception ex)
            {
                Log.Trace.Error(ex.Message, ex);
                ThreadPool.QueueUserWorkItem(_ => this.DisconnectAsyncCompleted(this, e));
                return;
            }

            if (!completedAsync)
                ThreadPool.QueueUserWorkItem(_ => this.DisconnectAsyncCompleted(this, e));
        }
        /// <summary>
        /// async disconnect callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            //dispose socket
            try { this._socket.Close(); }
            catch (Exception ex) { Log.Trace.Error(ex.Message, ex); }

            //dispose socketAsyncEventArgs
            var reason = e.UserToken as Exception;
            e.Completed -= this.DisconnectAsyncCompleted;
            e.Dispose();

            //fire disconnected
            this.OnDisconnected(reason);
            //close send queue
            this.FreeSendQueue();
        }
    }
}
