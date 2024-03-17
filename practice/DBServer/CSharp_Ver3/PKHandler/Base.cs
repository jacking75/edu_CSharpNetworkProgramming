using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

using ServerCommon;

namespace DBServer.PKHandler
{
    public class Base
    {
        public static UInt16 DBServerIndex;

        public MySqlConnection MySqlConnection;
        
        public Cache.Manager CacheMgr = new Cache.Manager();

        protected UInt16 ReqServerIndex;
        protected UInt64 ReqUserUniqueId;

        public virtual void Process(MQPacketHeadInfo mqHead, byte[] mqData) { }

    }
}
