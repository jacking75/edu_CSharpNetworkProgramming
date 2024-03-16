using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;


namespace FreeNetLite;


public class SocketAsyncEventArgsPool
{
    BufferManagerAsync BufferManager = new();
    ConcurrentBag<SocketAsyncEventArgs> Pool = new();


    /// 초기화
    /// bufferCount: 버퍼의 개수. 고정 크기 버퍼의 개수이다.  BufferManagerType.ArrayPool 방식에서는 사용하지 않는다
    /// bufferSize: arg 하나를 할당받을 때 사용하는 버퍼의 최대 크기이다. 
    ///            이것에 의해 전체 사용 메모리 크기는 bufferCount * maxBufferSize 이다
    public void Init(int bufferCount, int bufferSize)
    {
        BufferManager = new BufferManagerAsync();
        BufferManager.Init(bufferCount, bufferSize); 
    }

    /// arg에 버퍼를 할당하고, pool에 저장한다
    public void Allocate(SocketAsyncEventArgs arg)
    {
        BufferManager.SetBuffer(arg);
        Push(arg);
    }

    public void Push(SocketAsyncEventArgs arg)
    {
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


    
}
