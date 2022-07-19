using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FreeNet;

public class BufferManagerAsync : IBufferManager
{
    int TotalBytes;
    byte[] TotalBuffer;
    ConcurrentBag<int> FreeIndexPool = new();
    int TakeBufferSize;

    public void Init(int bufferCount, int bufferSize)
    {
        TotalBytes = bufferCount * bufferSize;
        TakeBufferSize = bufferSize;
        TotalBuffer = new byte[TotalBytes];

        var count = TotalBytes / TakeBufferSize;
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
