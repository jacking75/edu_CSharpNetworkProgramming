using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MGAsyncNet
{
    /// <summary>
    /// TODO:  1. ���� ����ϰ� ���� ����. ����ϵ��� �����ϱ�. ���� http://lab.gamecodi.com/board/zboard.php?id=GAMECODILAB_Lecture_series&no=61
    /// 2. �۷ι��ϰ� ������� ����, �� pool�� ���Ǻ��� �༭ �� ���Ǻ��� ����ϰ� �Ѵ�.
    /// </summary>
    class SocketAsyncEventArgsPool
    {
        ConcurrentBag<SocketAsyncEventArgs> m_Pool = new ConcurrentBag<SocketAsyncEventArgs>();
        
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }

            m_Pool.Add(item);
        }

        public SocketAsyncEventArgs Pop()
        {
            if(m_Pool.TryTake(out var item) == false)
            {
                return null;
            }

            return item;
        }

        public int Count { get { return m_Pool.Count; } }

    }
}
