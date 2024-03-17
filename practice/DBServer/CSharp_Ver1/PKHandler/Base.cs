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
        
        public virtual void Process(MQPacketHeadInfo mqHead, byte[] mqData) { }

    }
}
