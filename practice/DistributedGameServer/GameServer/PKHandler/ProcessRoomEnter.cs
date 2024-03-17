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
        void HandlerRequestRoomEnter(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: ReqRoomEnter");

            try
            {
                var user = UserMgr.GetUser(sessionID);
                if (user == null || user.IsConfirm(sessionID) == false)
                {
                    ResponseToClient<PKTResRoomEnter>(PacketID.RES_ROOM_ENTER, ErrorCode.ROOM_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsStateLogin() == false || user.IsStateRoom())
                {
                    ResponseToClient<PKTResRoomEnter>(PacketID.RES_ROOM_ENTER, ErrorCode.ROOM_INVALID_STATE, sessionID);
                    return;
                }

                var room = RoomMgr.GetRoom(user.MatchRoomNumber);
                if (room == null)
                {
                    ResponseToClient<PKTResRoomEnter>(PacketID.RES_ROOM_ENTER, ErrorCode.ROOM_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }


                if (room.AddUser(user.UserID, sessionID) == false)
                {
                    ResponseToClient<PKTResRoomEnter>(PacketID.RES_ROOM_ENTER, ErrorCode.ROOM_FAIL_ADD_USER, sessionID);
                    return;
                }

                user.EnteredRoom();

                ResponseToClient<PKTResRoomEnter>(PacketID.RES_ROOM_ENTER, ErrorCode.None, sessionID);


                room.NotifyPacketUserList(sessionID);
                room.NotifyPacketNewUser(sessionID, user.UserID);

                MainServer.GlobalLogger.Debug("Send: ResRoomEnter");
            }
            catch (Exception ex)
            {
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
