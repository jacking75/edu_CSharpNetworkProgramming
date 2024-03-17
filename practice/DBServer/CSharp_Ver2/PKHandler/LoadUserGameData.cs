using ServerCommon;

using System;
using System.Collections.Generic;
using System.Text;

using Dapper;
using MessagePack;
using MySqlConnector;


namespace DBServer.PKHandler
{
    class LoadUserGameData : Base
    {
        public LoadUserGameData(MySqlConnection mysqlConnection, Cache.Manager cacheMgr)
        {
            MySqlConnection = mysqlConnection;
            CacheMgr = cacheMgr;
        }

        public override void Process(MQPacketHeadInfo mqReqHeader, byte[] mqData)
        {
            try
            {
                ProcessImpl(mqReqHeader, mqData);
                Console.WriteLine("LoadUserGameData !!!! ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void ProcessImpl(MQPacketHeadInfo mqReqHeader, byte[] mqData)
        {
            ReqServerIndex = mqReqHeader.SenderIndex;
            ReqUserUniqueId = mqReqHeader.UserUniqueId;

            var reqData = MessagePackSerializer.Deserialize<MQReqUserGameDataLoad>(mqData);

            //TODO MySQL에서 유저의 게임 데이터를 읽어 온다

            Int64 userMoney = 23456;
            Int32 userDiamond = 110;
            var slot1 = new DBSlotInfo() { Index = 0, SkillCode = 21 };
            var slot2 = new DBSlotInfo() { Index = 3, SkillCode = 34 };
            var slot3 = new DBSlotInfo() { Index = 7, SkillCode = 55 };


            var user = CacheMgr.AddUser(reqData.UID);
            user.GameMoneyObj.Set(userMoney, userDiamond);
            user.QuickSlotObj.UpdateSlot(slot1);
            user.QuickSlotObj.UpdateSlot(slot2);
            user.QuickSlotObj.UpdateSlot(slot3);


            var resData = new MQResUserGameDataLoad();
            resData.Result = ErrorCode.None;
            resData.GameMoney = userMoney;
            resData.Diamond = userDiamond;
            resData.SlotList.Add(slot1);
            resData.SlotList.Add(slot2);
            resData.SlotList.Add(slot3);

            var resPacket = MessagePackSerializer.Serialize(resData);

            var mqresHeader = new MQPacketHeadInfo();
            mqresHeader.Id = (UInt16)MqPacketId.MQ_RES_LOAD_USER_GAME_DATA;
            mqresHeader.SenderIndex = Base.DBServerIndex;
            mqresHeader.UserUniqueId = ReqUserUniqueId;
            mqresHeader.Write(resPacket);
            mqresHeader.Write(resPacket);

            PacketProcessor.MQSendFunc(ReqServerIndex, resPacket);
        }
    }
}
