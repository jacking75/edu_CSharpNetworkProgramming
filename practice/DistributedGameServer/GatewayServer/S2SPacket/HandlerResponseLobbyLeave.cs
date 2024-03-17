using CSCommon;
using ServerCommon;
using ServerCommon.MQ;

using ZLogger;
using MessagePack;

using System;

namespace GatewayServer.S2SPacket
{
    public partial class Handler
    {
        public void ResponseLobbyLeave(PacketHeaderInfo mqHeader, byte[] packet)
        {
            var responseData = MessagePackSerializer.Deserialize<ResLobbyLeave>(packet);

            var session = ConnSessionMgrRef.GetSession(mqHeader.UID);
            if(session == null)
            {
                return;
            }

            if (responseData.Result == (Int16)SErrorCode.None)
            {
                session.LeaveLobby();

                SendResponseLobbyLeaveToClient((Int16)ErrorCode.None, session.NetSessionID);
            }
            else
            {
                SendResponseLobbyLeaveToClient((Int16)responseData.Result, session.NetSessionID);
            }
            
        }
        
        void SendResponseLobbyLeaveToClient(Int16 result, string netSessionID)
        {
            var responseData = new PKTResLobbyLeave()
            {
                Result = result,
            };

            var packetData = MessagePack.MessagePackSerializer.Serialize(responseData);
            MsgPackPacketHeaderInfo.Write(packetData,
                            (UInt16)packetData.Length,
                            CSCommon.PacketID.ResLobbyLeave );

            SendNetworkFunc(netSessionID, packetData);
        }
    }
}
