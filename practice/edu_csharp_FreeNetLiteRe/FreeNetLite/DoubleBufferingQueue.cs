using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreeNetLite;

/// <summary>
/// 두개의 큐를 교체해가며 활용한다.
/// IO스레드에서 입력큐에 막 쌓아놓고,
/// 로직스레드에서 큐를 뒤바꾼뒤(swap) 쌓아놓은 패킷을 가져가 처리한다.
/// 참고 : http://roadster.egloos.com/m/4199854
/// </summary>
class DoubleBufferingQueue
{
    // 실제 데이터가 들어갈 큐.
    Queue<Packet> Queue1;
    Queue<Packet> Queue2;

    // 각각의 큐에 대한 참조.
    Queue<Packet> RefInput;
    Queue<Packet> RefOutput;

    SpinLock LOCK = new SpinLock();

    public DoubleBufferingQueue()
    {
        // 초기 세팅은 큐와 참조가 1:1로 매칭되게 설정한다.
        Queue1 = new Queue<Packet>();
        Queue2 = new Queue<Packet>();
        RefInput = Queue1;
        RefOutput = Queue2;
    }


    /// <summary>
    /// IO스레드에서 전달한 패킷을 보관한다.
    /// </summary>
    /// <param name="msg"></param>
    public void Enqueue(Packet msg)
    {
        var gotLock = false;
        try
        {
            LOCK.Enter(ref gotLock);
            RefInput.Enqueue(msg);
        }
        finally
        {
            // Only give up the lock if you actually acquired it
            if (gotLock)
            {
                LOCK.Exit();
            }
        }           
    }


    public Queue<Packet> TakeAll()
    {
        swap();
        return RefOutput;
    }


    /// <summary>
    /// 입력큐와 출력큐를 뒤바꾼다.
    /// </summary>
    void swap()
    {
        var gotLock = false;
        try
        {
            LOCK.Enter(ref gotLock);

            var temp = RefInput;
            RefInput = RefOutput;
            RefOutput = temp;
        }
        finally
        {
            // Only give up the lock if you actually acquired it
            if (gotLock)
            {
                LOCK.Exit();
            }
        }
    }
}
