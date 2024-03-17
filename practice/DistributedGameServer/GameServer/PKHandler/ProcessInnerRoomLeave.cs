using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.PKHandler
{
    public partial class Process
    {
        void HandlerNtfInnerRoomLeave(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug($"Received Internal: HandlerNtfInnerRoomLeave. SessionID: {sessionID}");

            var reqData = MessagePackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.Data);
            LeaveRoomUser(sessionID, reqData.RoomNumber);
        }
    }
}
