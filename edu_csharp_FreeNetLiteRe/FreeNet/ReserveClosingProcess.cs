using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FreeNet
{
    class ReserveClosingProcess
    {
        ConcurrentQueue<Session> ReserveQueue = new ConcurrentQueue<Session>();

        bool IsStarted = false;
        Thread ProcessThread = null;

        int CheckMilliSecond = 100;

                
        public void Start(int checkMilliSecond)
        {
            CheckMilliSecond = checkMilliSecond;
            IsStarted = true;

            ProcessThread = new Thread(Process);
            ProcessThread.Start();
        }

        public void Stop()
        {
            if(IsStarted == false)
            {
                return;
            }

            IsStarted = false;
            ProcessThread.Join();
        }

        public void Add(Session session)
        {
            ReserveQueue.Enqueue(session);
        }


        void Process()
        {
            while (IsStarted)
            {
                var currentTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

                while (true)
                {
                    bool isTimeWaitOrEmpty = true;

                    if (ReserveQueue.TryPeek(out var temp) && temp.ReserveClosingMillSec <= currentTime)
                    {
                        if (ReserveQueue.TryDequeue(out var session))
                        {
                            isTimeWaitOrEmpty = false;
                            session.Close();
                        }
                    }

                    if(isTimeWaitOrEmpty)
                    {
                        break;
                    }
                }
                
                Thread.Sleep(CheckMilliSecond);
            }
        }
    }
}
