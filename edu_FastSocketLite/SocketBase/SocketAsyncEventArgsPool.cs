using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FastSocketLite.SocketBase
{
    internal class SocketAsyncEventArgsPool
    {
        private readonly int _messageBufferSize;
        private readonly ConcurrentStack<SocketAsyncEventArgs> _pool = new ConcurrentStack<SocketAsyncEventArgs>();

        /// <summary>
        /// new
        /// </summary>
        /// <param name="messageBufferSize"></param>
        public SocketAsyncEventArgsPool(int messageBufferSize)
        {
            this._messageBufferSize = messageBufferSize;
        }


        /// <summary>
        /// acquire
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Acquire()
        {
            SocketAsyncEventArgs e = null;
            if (this._pool.TryPop(out e))
            {
                return e;
            }

            e = new SocketAsyncEventArgs();
            e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
            return e;
        }

        /// <summary>
        /// release
        /// </summary>
        /// <param name="e"></param>
        public void Release(SocketAsyncEventArgs e)
        {
            //TODO 최소 pool 수 하드코딩을 설정 변수로 변경하기
            if (this._pool.Count < 10000)
            {
                this._pool.Push(e);
                return;
            }

            e.Dispose();
        }
    }
}
