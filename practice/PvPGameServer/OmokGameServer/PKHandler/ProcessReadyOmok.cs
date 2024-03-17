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
        void HandlerRequestReadyOmok(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: ReqReadyOmok");
            
            try
            {
                var (result, room, roomUser) = CheckRoomAndRoomUser(sessionID);
                if (result == false)
                {
                    ResponseToClient<PKTResReadyOmok>(PacketID.RES_READY_OMOK, ErrorCode.ROOM_INVALID_USER, sessionID);
                    return;
                }

                if (roomUser.State == Rooms.UserState.INGAME)
                {
                    ResponseToClient<PKTResReadyOmok>(PacketID.RES_READY_OMOK, ErrorCode.ROOM_INVALID_STATE, sessionID);
                    return;
                }


                // 준비 or 대기 상태로 변경
                room.NotifyReady(roomUser);
                
                
                // 모두 준비 상태이면, 오목 시작
                if (room.CheckReadyUsers())
                {
                    var firstUserID = room.StartOmok();
                    if (firstUserID == string.Empty)
                    {
                        MainServer.GlobalLogger.Debug("Error: Room.StartOmok");
                        return;
                    }

                    room.NotifyStart(firstUserID);

                    MainServer.GlobalLogger.Info($"Game Start. first:{firstUserID}");
                }
            }
            catch (Exception ex)
            {
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
