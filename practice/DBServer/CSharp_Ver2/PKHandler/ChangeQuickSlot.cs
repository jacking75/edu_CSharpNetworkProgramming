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
            var reqServerIndex = mqReqHeader.SenderIndex;
            var reqUserUniqueId = mqReqHeader.UserUniqueId;

            var reqData = MessagePackSerializer.Deserialize<MQReqChangeQuickSlot>(mqData);

            var userCacheData = CacheMgr.GetUser(ReqUserUniqueId);
            if (userCacheData == null)
            {
                return;
            }

            var slot = new DBSlotInfo() { Index = reqData.Index, SkillCode = reqData.SkillCode};
            var dbPolicyRet = userCacheData.QuickSlotObj.UpdateSlot(slot);

            if(dbPolicyRet == Cache.DBPolicy.UPDATE )
            {
                userCacheData.QuickSlotObj.SaveDB(MySqlConnection);
            }
            
            //TODO 결과 통보하기
        }
    }
}
