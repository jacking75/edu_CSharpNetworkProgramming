using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace FreeNet
{
    /// <summary>
    /// 현재 접속중인 전체 유저를 관리하는 클래스.
    /// </summary>
    public class SessionManager
    {
        // ConcureentDictionary를 사용한다. 그런데 foreach에서 스레드 세이프한지 테스트 하자. 
        // 스택오버플로어에서는 스레드 세이프하다고 한다.

        Int64 ConnectdSessionCount;

        ConcurrentDictionary<UInt64, Session> Sessions = new ConcurrentDictionary<UInt64, Session>();

        Timer TimerHeartbeat;
        long HeartbeatDuration;

                        
        public void Add(Session user)
        {
           if( Sessions.TryAdd(user.UniqueId, user))
            {
                Interlocked.Increment(ref ConnectdSessionCount);
            }
        }


        public void Remove(Session user)
        {
            var uniqueId = user.UniqueId;

            if(Sessions.TryRemove(uniqueId, out var temp))
            {
                Interlocked.Decrement(ref ConnectdSessionCount);
            }
        }


        public bool IsExist(Session user)
        {
            return Sessions.ContainsKey(user.UniqueId);
        }


        public Int64 CurrentConnectdSessionCount()
        {
            Interlocked.Read(ref ConnectdSessionCount);
            return ConnectdSessionCount;
        }


        public void StartHeartbeatChecking(uint check_interval_sec, uint allow_duration_sec)
        {
            HeartbeatDuration = allow_duration_sec * 10000000;
            TimerHeartbeat = new Timer(CheckHeartbeat, null, 1000 * check_interval_sec, 1000 * check_interval_sec);
        }

        void CheckHeartbeat(object state)
        {
            var allowed_time = (UInt64)(DateTime.Now.Ticks - this.HeartbeatDuration);

            foreach(var user in Sessions.Values)
            {
                var heartbeat_time = user.LatestHeartbeatTime;
                if (heartbeat_time >= allowed_time)
                {
                    continue;
                }

                user.DisConnect(true);
            }            
        }


    }
}
