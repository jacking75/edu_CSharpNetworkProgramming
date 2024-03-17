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
        void HandlerRequestLeave(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: ReqRoomLeave");

            try
            {
                var user = UserMgr.GetUser(sessionID);
                if (user == null)
                {
                    return;
                }

                if (LeaveRoomUser(sessionID, user.RoomNumber) == false)
                {
                    return;
                }

                user.LeaveRoom();

                ResponseToClient<PKTResRoomLeave>(PacketID.RES_ROOM_LEAVE, ErrorCode.None, sessionID);

                MainServer.GlobalLogger.Debug("Send: ResRoomLeave");
            }
            catch (Exception ex)
            {
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }

        bool LeaveRoomUser(string sessionID, int roomNumber)
        {
            MainServer.GlobalLogger.Debug($"LeaveRoomUser. SessionID: {sessionID}");

            var room = RoomMgr.GetRoom(roomNumber);
            if(room == null)
            {
                return false;
            }

            var roomUser = room.GetUserByNetSessionId(sessionID);
            if(roomUser == null)
            {
                return false;
            }


            var userID = roomUser.UserID;
            room.RemoveUser(roomUser);

            room.NotifyPacketLeaveUser(userID);
            return true;
        }
    }
}
