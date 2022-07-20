using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;


namespace FreeNetLite;

public enum SocketAsyncEventArgsPoolBufferMgrType
{
    Concurrent = 0,
    Lock = 1,
}

public class SocketAsyncEventArgsPool
{
    IBufferManager BufferManager = new BufferManagerAsync();
    ConcurrentBag<SocketAsyncEventArgs> Pool = new ConcurrentBag<SocketAsyncEventArgs>();


    /// 초기화
    ///<typeparam name="bufferCount">버퍼의 개수. 고정 크기 버퍼의 개수이다.  BufferManagerType.ArrayPool 방식에서는 사용하지 않는다</typeparam>
    ///<typeparam name="maxBufferSize"> arg 하나를 할당받을 때 사용하는 버퍼의 최대 크기이다. 이것에 의해 전체 사용 메모리 크기는 bufferCount * maxBufferSize 이다</typeparam>
    public void Init(int bufferCount, int maxBufferSize)
    {
        BufferManager = new BufferManagerAsync();
        BufferManager.Init(bufferCount, maxBufferSize); 
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


    
}
