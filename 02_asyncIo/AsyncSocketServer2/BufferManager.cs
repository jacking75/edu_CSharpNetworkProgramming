using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AsyncSocketServer2;

class BufferManager
{
    // 이 클래스는 하나의 큰 버퍼를 생성하고
    // 각 소켓 I/O 작업에 사용되는 SocketAsyncEventArgs 개체에 할당할 수 있도록 분할할 수 있습니다.
    // 이를 통해 버퍼를 쉽게 재사용할 수 있으며 힙 메모리의 단편화를 방지합니다.
    //
    // 이 버퍼는 Windows TCP 버퍼가 데이터를 복사할 수 있는 바이트 배열입니다.

    // 버퍼 풀이 제어하는 총 바이트 수
    Int32 totalBytesInBufferBlock;

    // 버퍼 매니저가 유지하는 바이트 배열
    byte[] bufferBlock;
    Stack<int> freeIndexPool;
    Int32 currentIndex;
    Int32 bufferBytesAllocatedForEachSaea;

    public BufferManager(Int32 totalBytes, Int32 totalBufferBytesInEachSaeaObject)
    {
        totalBytesInBufferBlock = totalBytes;
        this.currentIndex = 0;
        this.bufferBytesAllocatedForEachSaea = totalBufferBytesInEachSaeaObject;
        this.freeIndexPool = new Stack<int>();
    }

    // 버퍼 풀에서 사용되는 버퍼 공간을 할당합니다.
    internal void InitBuffer()
    {
        // 하나의 큰 버퍼 블록을 생성합니다.
        this.bufferBlock = new byte[totalBytesInBufferBlock];
    }

    // 하나의 큰 버퍼 블록을 각 SocketAsyncEventArg 개체에 할당합니다.
    // 지정된 SocketAsyncEventArgs 개체에 버퍼 공간을 할당합니다.
    //
    // 버퍼가 성공적으로 설정되었으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
    internal bool SetBuffer(SocketAsyncEventArgs args)
    {

        if (this.freeIndexPool.Count > 0)
        {
            // 이 if 문은 이전에 FreeBuffer를 호출하여 버퍼 공간의 오프셋을 이 스택에 다시 넣은 경우에만 true입니다.
            args.SetBuffer(this.bufferBlock, this.freeIndexPool.Pop(), this.bufferBytesAllocatedForEachSaea);
        }
        else
        {
            // 이 else 문에는 Init 메서드에서 SAEA 개체 풀이 구축될 때 각 SAEA 개체에 대한 버퍼를 설정하는 데 사용되는 코드가 있습니다.
            if ((totalBytesInBufferBlock - this.bufferBytesAllocatedForEachSaea) < this.currentIndex)
            {
                return false;
            }
            args.SetBuffer(this.bufferBlock, this.currentIndex, this.bufferBytesAllocatedForEachSaea);
            this.currentIndex += this.bufferBytesAllocatedForEachSaea;
        }
        return true;
    }

    // SocketAsyncEventArg 개체에서 버퍼를 제거합니다. 이렇게 하면 버퍼가 버퍼 풀로 반환됩니다.
    // 예외 처리의 경우를 제외하고는 FreeBuffer 메서드를 사용하지 않는 것이 좋습니다.
    // 대신 서버에서는 이 앱이 실행되는 동안 하나의 SAEA 개체에 대해 동일한 버퍼 공간을 유지합니다.
    internal void FreeBuffer(SocketAsyncEventArgs args)
    {
        this.freeIndexPool.Push(args.Offset);
        args.SetBuffer(null, 0, 0);
    }

}
