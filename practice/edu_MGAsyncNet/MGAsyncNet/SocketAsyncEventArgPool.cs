using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MGAsyncNet
{
    /// <summary>
    /// TODO:  1. 현재 사용하고 있지 않음. 사용하도록 수정하기. 참고 http://lab.gamecodi.com/board/zboard.php?id=GAMECODILAB_Lecture_series&no=61
    /// 2. 글로벌하게 사용하지 말고, 이 pool을 세션별로 줘서 각 세션별로 사용하게 한다.
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
