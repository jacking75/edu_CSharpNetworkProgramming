using Dapper;
using MessagePack;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerCommon;
using Microsoft.Extensions.Logging;

namespace DBServer.PKHandler
{
    class BuyItem : Base
    {
        public BuyItem(MySqlConnection mysqlConnection, Cache.Manager cacheMgr)
        {
            MySqlConnection = mysqlConnection;
            CacheMgr = cacheMgr;
        }

        public override void Process(MQPacketHeadInfo mqReqHeader, byte[] mqData)
        {
            try
            {
                ProcessImpl(mqReqHeader, mqData);
            }
            catch (Exception ex)
            {
                Program.GlobalLogger.LogError(ex.ToString());
            }
        }

       
        void ProcessImpl(MQPacketHeadInfo mqReqHeader, byte[] mqData)
        {
            ReqServerIndex = mqReqHeader.SenderIndex;
            ReqUserUniqueId = mqReqHeader.UserUniqueId;

            var reqData = MessagePackSerializer.Deserialize<MQReqBuyItem>(mqData);
             
            var userCacheData = CacheMgr.GetUser(ReqUserUniqueId);
            if(userCacheData == null)
            {
                SendRespnsePacket(ErrorCode.BuyItem_InvalidUser, reqData.ItemCode, 0);
                return; 
            }


            var item = GetItem(reqData.ItemCode);
            var newItemUID = InsertItem(reqData.UID, reqData.ItemCode);

            var moneyUpdateRet = userCacheData.GameMoneyObj.UpdateMoney(item.BuyMoney);
            var diamondUpdateRet = userCacheData.GameMoneyObj.UpdateDiamond(item.BuyDiamond);
            
            if(moneyUpdateRet == Cache.DBPolicy.UPDATE ||
                diamondUpdateRet == Cache.DBPolicy.UPDATE)
            {
                userCacheData.GameMoneyObj.SaveDB(MySqlConnection);
            }

            SendRespnsePacket(ErrorCode.None, reqData.ItemCode, newItemUID);
        }


        DBItem GetItem(UInt32 itemCode)
        {
            var dbItem = new DBItem();
            dbItem.Code = itemCode;
            dbItem.BuyMoney = 456;
            dbItem.BuyDiamond = 0;

            return dbItem;
        }

        UInt64 InsertItem(UInt64 uid, UInt32 itemId)
        {
            //DB에 아이템을 추가한다.

            return 1001;
        }

        void SendRespnsePacket(ErrorCode error, UInt32 itemID, UInt64 ItemUID)
        {
            var resData = new MQResBuyItem();
            resData.Result = error;
            resData.ItemID = itemID;
            resData.ItemUID = ItemUID;

            var resPacket = MessagePackSerializer.Serialize(resData);

            var mqresHeader = new MQPacketHeadInfo();
            mqresHeader.Id = (UInt16)MqPacketId.MQ_RES_BUY_ITEM;
            mqresHeader.SenderIndex = Base.DBServerIndex;
            mqresHeader.UserUniqueId = ReqUserUniqueId;
            mqresHeader.Write(resPacket);
            mqresHeader.Write(resPacket);

            PacketProcessor.MQSendFunc(ReqServerIndex, resPacket);
        }
    }
}
