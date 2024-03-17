using MessagePack;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Linq;

using ServerCommon;

namespace DBServer.PKHandler
{
    class GameRecord : Base
    {
        public GameRecord(MySqlConnection mysqlConnection)
        {
            MySqlConnection = mysqlConnection;
        }

        public override void Process(MQPacketHeadInfo mqHead, byte[] mqData)
        {
            try
            {
                ProcessImpl(mqHead, mqData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

       
        void ProcessImpl(MQPacketHeadInfo mqReqHeader, byte[] mqReqData )
        {
            var reqServerIndex = mqReqHeader.SenderIndex;
            var reqUserUniqueId = mqReqHeader.UserUniqueId;

            var reqData = MessagePackSerializer.Deserialize<MQReqGameRecord>(mqReqData);

            var reply = MySqlConnection.Query<MQResGameRecord>("select WinCount,LoseCount,DrawCount from OmokGameRecord where UID = @uid", new { uid = reqData.UserUID }).Single();


            var resPacket = MessagePackSerializer.Serialize(reply);

            var mqresHeader = new MQPacketHeadInfo();
            mqresHeader.Id = (UInt16)MqPacketId.MQ_RES_GAME_RECORD;
            mqresHeader.SenderIndex = Base.DBServerIndex;
            mqresHeader.UserUniqueId = reqUserUniqueId;
            mqresHeader.Write(resPacket);

            PacketProcessor.MQSendFunc(reqServerIndex, resPacket);

        }

    }
}
