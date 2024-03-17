using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GatewayServer.ConnSession
{
    public class Manager
    {
        static readonly ILogger<Manager> Logger = ServerCommon.LogManager.GetLogger<Manager>();


        ConcurrentDictionary<string, Session> NetIDSessionDic = new ConcurrentDictionary<string, Session>();
        ConcurrentDictionary<UInt64, Session> UIDSessionDic = new ();

        Int64 SequenceNum = 0;

        public bool Add(string netSessionID)
        {
            if(NetIDSessionDic.ContainsKey(netSessionID))
            {
                return false;
            }

            var uniqueID = (UInt64)Interlocked.Increment(ref SequenceNum);

            if (UIDSessionDic.ContainsKey(uniqueID))
            {
                return false;
            }

            var newSession = new Session();
            newSession.Init(uniqueID, netSessionID);

            NetIDSessionDic.TryAdd(netSessionID, newSession);
            UIDSessionDic.TryAdd(uniqueID, newSession);

            return true;
        }

        public void Remove(string netSessionID, UInt64 uid)
        {
            NetIDSessionDic.TryRemove(netSessionID, out var temp1);
            UIDSessionDic.TryRemove(uid, out var temp2);
        }

        public Session GetSession(string netSessionID)
        {
            NetIDSessionDic.TryGetValue(netSessionID, out var session);
            return session;
        }

        public Session GetSession(UInt64 uid)
        {
            UIDSessionDic.TryGetValue(uid, out var session);
            return session;
        }

        public void ChangeStateToLogin(string netSessionID)
        {
            var session = GetSession(netSessionID);
            if(session == null)
            {
                // 발생할 수 없는 에러
                Logger.ZLogError($"[ChangeStateToLogin] Invalid sessionID:{netSessionID}");
            }

            session.SetStateToLogin();
        }
    }
}
