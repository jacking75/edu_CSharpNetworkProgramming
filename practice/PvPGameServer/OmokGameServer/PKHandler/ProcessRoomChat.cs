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
        void HandlerRequestChat(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: ReqRoomChat");

            try
            {
                var (result, room, roomUser) = CheckRoomAndRoomUser(sessionID);
                if (result == false)
                {
                    ResponseToClient<PKTResRoomChat>(PacketID.RES_ROOM_CHAT, ErrorCode.ROOM_INVALID_USER, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomChat>(packetData.Data);

                room.Chat(roomUser.UserID, reqData.ChatMessage);

                MainServer.GlobalLogger.Debug("Send: NtfRoomChat");
            }
            catch (Exception ex)
            {
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
