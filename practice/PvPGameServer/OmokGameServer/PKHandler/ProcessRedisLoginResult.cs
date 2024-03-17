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
        void HandlerRequestRedisLoginResult(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: ReqRedisLoginResult");
            try
            {
                if (UserMgr.GetUser(sessionID) == null)
                {
                    ResponseToClient<PKTResLogin>(PacketID.RES_LOGIN, ErrorCode.LOGIN_NOT_FOUND_USER, packetData.SessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTResRedisLogin>(packetData.Data);
                if (reqData.Result != (short)ErrorCode.None)
                {
                    ResponseToClient<PKTResLogin>(PacketID.RES_LOGIN, (ErrorCode)reqData.Result, packetData.SessionID);
                    return;
                }

                var errorCode = UserMgr.SetLogin(sessionID, reqData.UserID, reqData.RoomNum);
                if (errorCode != ErrorCode.None)
                {
                    ResponseToClient<PKTResLogin>(PacketID.RES_LOGIN, errorCode, packetData.SessionID);
                    if (errorCode == ErrorCode.LOGIN_FULL_USER_COUNT)
                    {
                        ResponseToClient<PKTResLogin>(PacketID.NTF_MUST_CLOSE, ErrorCode.LOGIN_FULL_USER_COUNT, packetData.SessionID);
                    }
                    return;
                }

                ResponseToClient<PKTResLogin>(PacketID.RES_LOGIN, errorCode, packetData.SessionID);
                MainServer.GlobalLogger.Debug($"Send: ResLogin. UserID:{reqData.UserID}, {(uint)errorCode}");
            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
