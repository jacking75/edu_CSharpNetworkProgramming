using Microsoft.Extensions.Logging;
using ZLogger;

using MessagePack;
using ServerCommon;
using ServerCommon.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer.PKHandler
{
    public class GatewayLogin : Base
    {
        public static readonly ILogger<GatewayLogin> Logger = LogManager.GetLogger<GatewayLogin>();

        public GatewayLogin(DBMysql sql, DBRedis redis)
        {
            SQLDB = sql;
            RedisDB = redis;
        }

        public override void Process(PacketDataParams para)
        {
            try
            {
                ProcessImpl(para);
                Logger.ZLogDebug("[GatewayLogin] - Success");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex, "");
            }
        }

        void ProcessImpl(PacketDataParams para)
        {
            var requestData = MessagePackSerializer.Deserialize<ReqGatewayLogin>(para.MQData);

            var loginRet = RedisDB.IsSuccessGatewayLogin(requestData.UserID, requestData.AuthToken);


            MessagePackSerializer.Serialize(para.EncodingStream, new ResGatewayLogin()
            {
                Result = (Int16)loginRet, 
                UserID = requestData.UserID
            });             

            var responseHeader = new PacketHeaderInfo
            {
                ID = (UInt16)PacketID.ResGatewayLogin,
                SenderIndex = para.MyServerIndex,
                LobbyNumber = 0,
                RoomNumber = 0,
                UID = para.MQHeader.UID,
            };

            var sendDataSize = (int)para.EncodingStream.Position;
            responseHeader.Write(para.EncodingBuffer);
            
            var subject = SubjectManager.ToGatewayServer(para.MQHeader.SenderIndex);
            MQSendFunc(para.MQIndex, subject, para.EncodingBuffer, sendDataSize);
        }
    }
}
