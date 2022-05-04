using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FastSocketLite.SocketBase
{
    internal class PacketQueue
    {
        public const int IDLE = 1;     
        public const int SENDING = 2; 
        public const int ENQUEUE = 3; 
        public const int DEQUEUE = 4; 
        public const int CLOSED = 5;   

        private int _state = IDLE;      
        private Queue<Packet> _queue = new Queue<Packet>();
        private Action<Packet> _sendAction = null;


        /// <summary>
        /// new
        /// </summary>
        /// <param name="sendAction"></param>
        /// <exception cref="ArgumentNullException">sendAction is null.</exception>
        public PacketQueue(Action<Packet> sendAction)
        {
            if (sendAction == null)
            {
                throw new ArgumentNullException("sendAction");
            }
            this._sendAction = sendAction;
        }


        /// <summary>
        /// try send packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>if CLOSED return false.</returns>
        public bool TrySend(Packet packet)
        {
            var spin = true;
            while (spin)
            {
                switch (this._state)
                {
                    case IDLE:
                        if (Interlocked.CompareExchange(ref this._state, SENDING, IDLE) == IDLE)
                        {
                            spin = false;
                        }
                        break;
                    case SENDING:
                        if (Interlocked.CompareExchange(ref this._state, ENQUEUE, SENDING) == SENDING)
                        {
                            this._queue.Enqueue(packet);
                            this._state = SENDING;
                            return true;
                        }
                        break;
                    case ENQUEUE:
                    case DEQUEUE:
                        Thread.Yield();
                        break;
                    case CLOSED:
                        return false;
                }
            }
            this._sendAction(packet);
            return true;
        }

        /// <summary>
        /// close
        /// </summary>
        /// <returns></returns>
        public CloseResult Close()
        {
            var spin = true;
            int beforeState = -1;
            while (spin)
            {
                switch (this._state)
                {
                    case IDLE:
                        if (Interlocked.CompareExchange(ref this._state, CLOSED, IDLE) == IDLE)
                        {
                            spin = false;
                            beforeState = IDLE;
                        }
                        break;
                    case SENDING:
                        if (Interlocked.CompareExchange(ref this._state, CLOSED, SENDING) == SENDING)
                        {
                            spin = false;
                            beforeState = SENDING;
                        }
                        break;
                    case ENQUEUE:
                    case DEQUEUE:
                        Thread.Yield();
                        break;
                    case CLOSED:
                        return new CloseResult(CLOSED, null);
                }
            }

            // 다른 스레드에서 같은 타이밍에 queue를 접근하지는 않음.
            var arrPackets = this._queue.ToArray();
            this._queue.Clear();
            this._queue = null;
            // this._sendAction = null; 이 부분을 null로 하면 다른 스레드에서 TrySendNext()에서 SENDING 후 this._sendAction를 호출할 때 충돌한다.
            return new CloseResult(beforeState, arrPackets);
        }

        /// <summary>
        /// try send next packet
        /// </summary>
        /// <returns>if CLOSED return false.</returns>
        public bool TrySendNext()
        {
            var spin = true;
            Packet packet = null;
            while (spin)
            {
                switch (this._state)
                {
                    case SENDING:
                        if (Interlocked.CompareExchange(ref this._state, DEQUEUE, SENDING) == SENDING)
                        {
                            if (this._queue.Count == 0)
                            {
                                this._state = IDLE;
                                return true;
                            }

                            packet = this._queue.Dequeue();
                            this._state = SENDING;
                            spin = false;
                        }
                        break;
                    case ENQUEUE:
                        Thread.Yield();
                        break;
                    case CLOSED:
                        return false;
                }
            }
            this._sendAction(packet);
            return true;
        }


        /// <summary>
        /// close queue result
        /// </summary>
        public sealed class CloseResult
        {
            /// <summary>
            /// before close state
            /// </summary>
            public readonly int BeforeState;
            /// <summary>
            /// wait sending packet array
            /// </summary>
            public readonly Packet[] Packets;

            /// <summary>
            /// new
            /// </summary>
            /// <param name="beforeState"></param>
            /// <param name="packets"></param>
            public CloseResult(int beforeState, Packet[] packets)
            {
                this.BeforeState = beforeState;
                this.Packets = packets;
            }
        }
    }
}
