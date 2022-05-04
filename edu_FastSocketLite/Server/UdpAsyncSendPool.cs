using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FastSocketLite.Server
{
    internal class AsyncSendPool
    {
        private const int MAXPOOLSIZE = 3000;
        private readonly int _messageBufferSize;
        private readonly Socket _socket = null;
        private readonly ConcurrentStack<SocketAsyncEventArgs> _stack =  new ConcurrentStack<SocketAsyncEventArgs>();
        

        /// <summary>
        /// new
        /// </summary>
        /// <param name="messageBufferSize"></param>
        /// <param name="socket"></param>
        public AsyncSendPool(int messageBufferSize, Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            this._messageBufferSize = messageBufferSize;
            this._socket = socket;
        }
        

        
        /// <summary>
        /// send completed handle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.Release(e);
        }
        


        
        /// <summary>
        /// acquire
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Acquire()
        {
            SocketAsyncEventArgs e;
            if (this._stack.TryPop(out e))
            {
                return e;
            }

            e = new SocketAsyncEventArgs();
            e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
            e.Completed += this.SendCompleted;
            return e;
        }
        /// <summary>
        /// release
        /// </summary>
        /// <param name="e"></param>
        public void Release(SocketAsyncEventArgs e)
        {
            if (this._stack.Count >= MAXPOOLSIZE)
            {
                e.Completed -= this.SendCompleted;
                e.Dispose();
                return;
            }

            this._stack.Push(e);
        }
        /// <summary>
        /// sned async
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="payload"></param>
        /// <exception cref="ArgumentNullException">endPoint is null</exception>
        /// <exception cref="ArgumentNullException">payload is null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">payload length大于messageBufferSize</exception>
        public void SendAsync(EndPoint endPoint, byte[] payload)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }

            if (payload == null || payload.Length == 0)
            {
                throw new ArgumentNullException("payload");
            }

            if (payload.Length > this._messageBufferSize)
                throw new ArgumentOutOfRangeException("payload.Length", "payload length大于messageBufferSize");

            var e = this.Acquire();
            e.RemoteEndPoint = endPoint;

            Buffer.BlockCopy(payload, 0, e.Buffer, 0, payload.Length);
            e.SetBuffer(0, payload.Length);

            if (!this._socket.SendToAsync(e))
                ThreadPool.QueueUserWorkItem(_ => this.Release(e));
        }
        
    }
}
