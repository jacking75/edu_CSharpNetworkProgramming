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
        void HandlerRequestRedisNewMatchRoom(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.GlobalLogger.Debug("Received: RequestRedisNewMatchRoom");
            try
            {
                var ntfData = MessagePackSerializer.Deserialize<PKTNtfRedisMatchingRoom>(packetData.Data);

                var room = GetRoom(ntfData.RoomNum);
                if(room == null)
                {
                    MainServer.GlobalLogger.Error($"Invalid Room. RoomNum:{ntfData.RoomNum}");
                    return;
                }


                room.SetMatchingRoomTime();
            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.GlobalLogger.Error(ex.ToString());
            }
        }
    }
}
