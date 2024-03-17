using CSCommon;
using ServerCommon;
using ServerCommon.MQ;

using ZLogger;
using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayServer.S2SPacket
{
    public partial class Handler
    {
        public void ResponseLoobyRoomMQInfo(PacketHeaderInfo mqHeader, byte[] packet)
        {
            var responseData = MessagePackSerializer.Deserialize<ResLobbyRoomMQInfo>(packet);

            MQSubjectMgrRef.SetLobbyMQAddress(responseData.LobbyList);
            MQSubjectMgrRef.SetRoomMQAddress(responseData.RoomList);

            foreach(var lobby in responseData.LobbyList)
            {
                Logger.ZLogInformation($"[ResponseLoobyRoomMQInfo] LoobyMQInfo - LobbyIndex:{lobby.ServerIndex}, StartNum:{lobby.StartNum}, LastNum:{lobby.LastNum}");
            }

            foreach (var room in responseData.RoomList)
            {
                Logger.ZLogInformation($"[ResponseLoobyRoomMQInfo] RoomMQInfo - LobbyIndex:{room.ServerIndex}, StartNum:{room.StartNum}, LastNum:{room.LastNum}");
            }
        }

        
    }
}
