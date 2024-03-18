using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AsyncSocketServer2;

class BufferManager
{
    // �� Ŭ������ �ϳ��� ū ���۸� �����ϰ�
    // �� ���� I/O �۾��� ���Ǵ� SocketAsyncEventArgs ��ü�� �Ҵ��� �� �ֵ��� ������ �� �ֽ��ϴ�.
    // �̸� ���� ���۸� ���� ������ �� ������ �� �޸��� ����ȭ�� �����մϴ�.
    //
    // �� ���۴� Windows TCP ���۰� �����͸� ������ �� �ִ� ����Ʈ �迭�Դϴ�.

    // ���� Ǯ�� �����ϴ� �� ����Ʈ ��
    Int32 totalBytesInBufferBlock;

    // ���� �Ŵ����� �����ϴ� ����Ʈ �迭
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

    // ���� Ǯ���� ���Ǵ� ���� ������ �Ҵ��մϴ�.
    internal void InitBuffer()
    {
        // �ϳ��� ū ���� ����� �����մϴ�.
        this.bufferBlock = new byte[totalBytesInBufferBlock];
    }

    // �ϳ��� ū ���� ����� �� SocketAsyncEventArg ��ü�� �Ҵ��մϴ�.
    // ������ SocketAsyncEventArgs ��ü�� ���� ������ �Ҵ��մϴ�.
    //
    // ���۰� ���������� �����Ǿ����� true�� ��ȯ�ϰ�, �׷��� ������ false�� ��ȯ�մϴ�.
    internal bool SetBuffer(SocketAsyncEventArgs args)
    {

        if (this.freeIndexPool.Count > 0)
        {
            // �� if ���� ������ FreeBuffer�� ȣ���Ͽ� ���� ������ �������� �� ���ÿ� �ٽ� ���� ��쿡�� true�Դϴ�.
            args.SetBuffer(this.bufferBlock, this.freeIndexPool.Pop(), this.bufferBytesAllocatedForEachSaea);
        }
        else
        {
            // �� else ������ Init �޼��忡�� SAEA ��ü Ǯ�� ����� �� �� SAEA ��ü�� ���� ���۸� �����ϴ� �� ���Ǵ� �ڵ尡 �ֽ��ϴ�.
            if ((totalBytesInBufferBlock - this.bufferBytesAllocatedForEachSaea) < this.currentIndex)
            {
                return false;
            }
            args.SetBuffer(this.bufferBlock, this.currentIndex, this.bufferBytesAllocatedForEachSaea);
            this.currentIndex += this.bufferBytesAllocatedForEachSaea;
        }
        return true;
    }

    // SocketAsyncEventArg ��ü���� ���۸� �����մϴ�. �̷��� �ϸ� ���۰� ���� Ǯ�� ��ȯ�˴ϴ�.
    // ���� ó���� ��츦 �����ϰ�� FreeBuffer �޼��带 ������� �ʴ� ���� �����ϴ�.
    // ��� ���������� �� ���� ����Ǵ� ���� �ϳ��� SAEA ��ü�� ���� ������ ���� ������ �����մϴ�.
    internal void FreeBuffer(SocketAsyncEventArgs args)
    {
        this.freeIndexPool.Push(args.Offset);
        args.SetBuffer(null, 0, 0);
    }

}
