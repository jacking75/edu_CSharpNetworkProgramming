using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;


namespace FreeNet
{
    public enum SocketAsyncEventArgsPoolBufferMgrType
    {
        Concurrent = 0,
        Lock = 1,
    }

    public class SocketAsyncEventArgsPool
    {
        IBufferManager BufferManager = new BufferManagerAsync();
        ConcurrentBag<SocketAsyncEventArgs> Pool = new ConcurrentBag<SocketAsyncEventArgs>();


        /// <summary>
        /// 초기화
        /// </summary>
        /// <typeparam name="bufferMgrType">버퍼 메모리 관리 방식을 선택한다. enum BufferManagerType</typeparam>
        ///<typeparam name="bufferCount">버퍼의 개수. 고정 크기 버퍼의 개수이다.  BufferManagerType.ArrayPool 방식에서는 사용하지 않는다</typeparam>
        ///<typeparam name="maxBufferSize"> arg 하나를 할당받을 때 사용하는 버퍼의 최대 크기이다. 이것에 의해 전체 사용 메모리 크기는 bufferCount * maxBufferSize 이다</typeparam>
        public void Init(SocketAsyncEventArgsPoolBufferMgrType bufferMgrType, int bufferCount, int maxBufferSize)
        {
            if (bufferMgrType == SocketAsyncEventArgsPoolBufferMgrType.Concurrent)
            {
                BufferManager = new BufferManagerAsync();
                BufferManager.Init(bufferCount, maxBufferSize);
            }
            else
            {
                BufferManager = new BufferManagerSynk();
                BufferManager.Init(bufferCount, maxBufferSize);
            }
        }

        /// <summary>
        /// 초기화. arg에 초기화 작업을 하고, pool에 저장한다
        /// </summary>
        /// <typeparam name="arg">SocketAsyncEventArgs</typeparam>
        public void Allocate(SocketAsyncEventArgs arg)
        {
            BufferManager.SetBuffer(arg);
            Push(arg);
        }

        public void Push(SocketAsyncEventArgs arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }

            Pool.Add(arg);
        }

        public SocketAsyncEventArgs Pop()
        {
            if (Pool.TryTake(out var result))
            {
                return result;
            }

            return null;
        }




        internal interface IBufferManager
        {
            void Init(int bufferCountOrMaxTakeBufferSize, int bufferSizeOrbucketCount);
            bool SetBuffer(SocketAsyncEventArgs args);
            void FreeBuffer(SocketAsyncEventArgs args);
        }

        public class BufferManagerAsync : IBufferManager
        {
            int NumBytes;
            byte[] TotalBuffer;
            ConcurrentBag<int> FreeIndexPool = new ConcurrentBag<int>();
            int TakeBufferSize;

            public void Init(int bufferCount, int bufferSize)
            {
                NumBytes = bufferCount * bufferSize;
                TakeBufferSize = bufferSize;
                TotalBuffer = new byte[NumBytes];

                var count = NumBytes / TakeBufferSize;
                for (int i = 0; i < count; ++i)
                {
                    FreeIndexPool.Add((i * TakeBufferSize));
                }
            }

            public bool SetBuffer(SocketAsyncEventArgs args)
            {
                if (FreeIndexPool.TryTake(out int index))
                {
                    args.SetBuffer(TotalBuffer, index, TakeBufferSize);
                    return true;
                }

                return false;
            }

            public void FreeBuffer(SocketAsyncEventArgs args)
            {
                FreeIndexPool.Add(args.Offset);
                args.SetBuffer(null, 0, 0);
                args.Dispose();
            }
        }


        class BufferManagerSynk : IBufferManager
        {
            int NumBytes;                 // the total number of bytes controlled by the buffer pool  
            byte[] TotalBuffer;                // the underlying byte array maintained by the Buffer Manager  
            Stack<int> FreeIndexPool;     //   
            int CurrentIndex;
            int OneTaskBufferSize;


            /// <summary>  
            /// Allocates buffer space used by the buffer pool  
            /// </summary>  
            public void Init(int totalBytes, int bufferSize)
            {
                NumBytes = totalBytes;
                CurrentIndex = 0;
                OneTaskBufferSize = bufferSize;
                FreeIndexPool = new Stack<int>();

                TotalBuffer = new byte[NumBytes];
            }

            /// <summary>  
            /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object  
            /// </summary>  
            /// <returns>true if the buffer was successfully set, else false</returns>  
            public bool SetBuffer(SocketAsyncEventArgs args)
            {
                if (FreeIndexPool.Count > 0)
                {
                    args.SetBuffer(TotalBuffer, FreeIndexPool.Pop(), OneTaskBufferSize);
                }
                else
                {
                    if ((NumBytes - OneTaskBufferSize) < CurrentIndex)
                    {
                        return false;
                    }
                    args.SetBuffer(TotalBuffer, CurrentIndex, OneTaskBufferSize);
                    CurrentIndex += OneTaskBufferSize;
                }
                return true;
            }

            /// <summary>  
            /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the   
            /// buffer pool  
            /// </summary>  
            public void FreeBuffer(SocketAsyncEventArgs args)
            {
                FreeIndexPool.Push(args.Offset);
                args.SetBuffer(null, 0, 0);
                args.Dispose();
            }
        }
    }
}
