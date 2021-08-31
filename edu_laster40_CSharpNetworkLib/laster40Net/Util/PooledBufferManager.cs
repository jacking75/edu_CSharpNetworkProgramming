using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace laster40Net.Util
{
    public interface IBufferManager
    {
        byte[] Take(int size);
        void Return(byte[] buffer);
    }

    /// <summary>
    /// 그냥 버퍼 관리자 (GC)
    /// </summary>
    public class GCBufferManager : IBufferManager
    {
        public byte[] Take(int size)
        {
            return new byte[size];
        }

        public void Return(byte[] buffer)
        {
        }
    }

    /// <summary>
    /// 버퍼를 풀링하는 관리자
    /// 여러사이즈의 버퍼풀을 관리하도록 구현
    /// 쓰레드 세이프 하다.
    /// </summary>
    public class PooledBufferManager : IBufferManager
    {
        private class PooledBuffer : IBufferManager
        {
            public int _allocSize;
            public int AllocCount { get { return _allocCount; } }
            private int _allocCount;
            private int _hitsCount;
            private int _missesCount;
            ConcurrentBag<byte[]> _buffers;

            public PooledBuffer(int size)
            {
                _allocCount = 0;
                _hitsCount = 0;
                _missesCount = 0;
                _buffers = new ConcurrentBag<byte[]>();

                _allocSize = size;
            }

            public byte[] Take(int size)
            {
                bool fill = false;
                byte[] buffer;
                while (!_buffers.TryTake(out buffer))
                {
                    fill = true;
                    FillBuffer();
                }
                if (fill)
                {
                    Interlocked.Increment(ref _missesCount);
                }
                else
                {
                    Interlocked.Increment(ref _hitsCount);
                }
                return buffer;
            }

            public void Return(byte[] buffer)
            {
                _buffers.Add(buffer);
            }

            private void FillBuffer()
            {
                try
                {
                    _buffers.Add(AllocNewBuffer(_allocSize));

                    Interlocked.Increment(ref _allocCount);
                }
                catch (Exception)
                {
                    Console.WriteLine("Alloc - size:{0},count:{1}", _allocSize, _allocCount);
                }
            }

            public string Dump()
            {
                return string.Format("alloc size:{0} - count:{1}, free:{2}, hit:{3}, miss:{4}", _allocSize, _allocCount, _buffers.Count, _hitsCount, _missesCount);
            }

            public static byte[] AllocNewBuffer(int size)
            {
                return new byte[size];
            }
        }

        /// <summary>
        /// 버퍼 풀들
        /// </summary>
        private PooledBuffer[] _pools = null;

        /// <summary>
        /// 생성자 
        /// </summary>
        /// <param name="sizeArray">사이즈에 해당하는 풀을 생성함</param>
        public PooledBufferManager(int[] sizeArray)
        {
            Array.Sort(sizeArray);

            _pools = new PooledBuffer[sizeArray.Length];
            for (int i = 0; i < sizeArray.Length; ++i)
            {
                _pools[i] = new PooledBuffer(sizeArray[i]);
            }
        }

        /// <summary>
        /// 메모리 획득하기
        /// </summary>
        /// <param name="size">획득할 메모리 사이즈, 만약 풀에 존재하지 않는 값이면 일반 Alloctor로 할당해서 넘김</param>
        /// <returns>할당한 buffer </returns>
        public byte[] Take(int size)
        {
            PooledBuffer pooled = FindPool(size);
            if (pooled == null)
                return PooledBuffer.AllocNewBuffer(size);

            return pooled.Take(size);
        }
        /// <summary>
        /// 메모리 반납하기
        /// </summary>
        /// <param name="buffer">반납할 buffer</param>
        public void Return(byte[] buffer)
        {
            PooledBuffer pooled = FindPool(buffer.Length);
            if (pooled != null)
            {
                pooled.Return(buffer);
            }
        }
        /// <summary>
        /// 현재 메모리풀들의 상태를 string으로 반환해서 넘긴다.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var buffer in _pools)
            {
                builder.Append(buffer.Dump());
                builder.Append("\r\n");
            }

            return builder.ToString();
        }

        private PooledBuffer FindPool(int size)
        {
            foreach (var buffer in _pools)
            {
                if (size <= buffer._allocSize)
                    return buffer;
            }

            return null;
        }


    }
}
