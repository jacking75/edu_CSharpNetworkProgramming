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
        void HandlerRequestPutMok(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: ReqPutMok");
            try
            {
                var (result, room, roomUser) = CheckRoomAndRoomUser(sessionID);
                if (result == false)
                {
                    ResponseToClient<PKTResPutMok>(PacketID.RES_PUT_MOK, ErrorCode.ROOM_INVALID_USER, sessionID);
                    return;
                }

                if (roomUser.State != Rooms.UserState.INGAME)
                {
                    ResponseToClient<PKTResPutMok>(PacketID.RES_PUT_MOK, ErrorCode.ROOM_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqPutMok>(packetData.Data);


                // 목 두기
                var errorCode = room.PutMok(sessionID, roomUser.UserMok, reqData.PosX, reqData.PosY);
                if (errorCode != ErrorCode.None)
                {
                    ResponseToClient<PKTResPutMok>(PacketID.RES_PUT_MOK, errorCode, sessionID);
                    return;
                }


                room.CheckEndOmok(roomUser.UserMok);
            }
            catch (Exception ex)
            {
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
