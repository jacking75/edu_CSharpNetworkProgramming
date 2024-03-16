using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;

//TODO SocketNetUtil 사용하기
namespace MGAsyncNet
{
    class BufferManagerAsync
    {
        int NumBytes;              
        byte[] TotalBuffer;
        ConcurrentBag<int> FreeIndexPool = new ConcurrentBag<int>();        
        int TakeBufferSize;

        public BufferManagerAsync(int bufferCount, int bufferSize)
        {
            NumBytes = bufferCount * bufferSize;
            TakeBufferSize = bufferSize;
        }

        public void InitBuffer()
        {
            TotalBuffer = new byte[NumBytes];

            var count = NumBytes / TakeBufferSize;
            for( int i = 0; i < count; ++i)
            {
                FreeIndexPool.Add((i * TakeBufferSize));
            }
        }

        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if(FreeIndexPool.TryTake(out int index))
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



    class BufferManagerSynk
    {
        int NumBytes;                 // the total number of bytes controlled by the buffer pool  
        byte[] TotalBuffer;                // the underlying byte array maintained by the Buffer Manager  
        Stack<int> FreeIndexPool;     //   
        int CurrentIndex;
        int OneTaskBufferSize;

        public BufferManagerSynk(int totalBytes, int bufferSize)
        {
            NumBytes = totalBytes;
            CurrentIndex = 0;
            OneTaskBufferSize = bufferSize;
            FreeIndexPool = new Stack<int>();
        }

        /// <summary>  
        /// Allocates buffer space used by the buffer pool  
        /// </summary>  
        public void InitBuffer()
        {
            // create one big large buffer and divide that out to each SocketAsyncEventArg object  
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


    class BufferManagerArrayPool
    {
        System.Buffers.ArrayPool<byte> Pool;
        int MaxArraySize;
        int BucketCount;

        public BufferManagerArrayPool(int maxBufferSize, int bucketCount)
        {
            MaxArraySize = maxBufferSize;
            BucketCount = bucketCount;
        }

        /// <summary>  
        /// Allocates buffer space used by the buffer pool  
        /// </summary>  
        public void InitBuffer()
        {
            Pool = System.Buffers.ArrayPool<byte>.Create(MaxArraySize, BucketCount);
        }

        /// <summary>  
        /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object  
        /// </summary>  
        /// <returns>true if the buffer was successfully set, else false</returns>  
        public bool SetBuffer(SocketAsyncEventArgs args, int bufferSize)
        {
            var rentBuffer = Pool.Rent(bufferSize);
            args.SetBuffer(rentBuffer, 0, bufferSize);
            return true;
        }

        /// <summary>  
        /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the   
        /// buffer pool  
        /// </summary>  
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            //TODO 올바른 동작인지 확인 필요
            var buf = args.Buffer;            
            Pool.Return(buf);
                        
            args.SetBuffer(null, 0, 0);
            args.Dispose();
        }
    }
}
