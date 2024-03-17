using MessagePack;
using PvPGameServer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.PKHandler
{
    public partial class Process
    {
        void HandlerRequestLogin(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: HandlerRequestLogin");
            try
            {
                if (UserMgr.GetUser(sessionID) == null)
                {
                    ResponseToClient<PKTResLogin>(PacketID.RES_LOGIN, ErrorCode.LOGIN_NOT_FOUND_USER, packetData.SessionID);
                    return;
                }

                // 로그인 요청을 Redis에 전달한다.
                var reqData = MessagePackSerializer.Deserialize<PKTReqLogin>(packetData.Data);

                var reqLoginTask = new ServerCommon.Redis.ReqLoginTask
                {
                    NetSessionID = packetData.SessionID,
                    UserID = reqData.UserID,
                    AuthToken = reqData.AuthToken
                };
                var sendData = MessagePackSerializer.Serialize(reqLoginTask);
                ServerCommon.Redis.MsgPackHeaderInfo.WriteID(sendData, ServerCommon.Redis.MsgID.ReqLogin);
                PushRedisTaskFunc(sendData);

                MainServer.GlobalLogger.Debug($"Send Redis: ReqLoginTask UserID:{reqData.UserID}");
            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
