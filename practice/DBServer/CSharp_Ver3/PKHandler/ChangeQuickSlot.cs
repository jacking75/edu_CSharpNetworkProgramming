using Dapper;
using MessagePack;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerCommon;


namespace DBServer.PKHandler
{
    class ChangeQuickSlot : Base
    {
        public ChangeQuickSlot(MySqlConnection mysqlConnection, Cache.Manager cacheMgr)
        {
            MySqlConnection = mysqlConnection;
            CacheMgr = cacheMgr;
        }

        public override void Process(MQPacketHeadInfo mqReqHeader, byte[] mqData)
        {
            try
            {
                ProcessImpl(mqReqHeader, mqData);
                Console.WriteLine("ChangeQuickSlot !!!! ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

       
        void ProcessImpl(MQPacketHeadInfo mqReqHeader, byte[] mqData)
        {
            
        }
    }
}
