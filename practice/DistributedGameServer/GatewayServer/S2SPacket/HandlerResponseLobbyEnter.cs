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
        public void ResponseLobbyEnter(PacketHeaderInfo mqHeader, byte[] packet)
        {
            var responseData = MessagePackSerializer.Deserialize<ResLobbyEnter>(packet);

            var session = ConnSessionMgrRef.GetSession(mqHeader.UID);
            if(session == null)
            {
                return;
            }

            if (responseData.Result == (Int16)SErrorCode.None)
            {
                session.EnterLobby(mqHeader.SenderIndex, responseData.LobbyNumber);

                SendResponseLobbyEnterToClient(session.NetSessionID, (Int16)ErrorCode.None, responseData.LobbyNumber);
            }
            else
            {
                session.EnterLobbyFail();

                SendResponseLobbyEnterToClient(session.NetSessionID, responseData.Result, responseData.LobbyNumber);
            }
            
        }
                
        void SendResponseLobbyEnterToClient(string netSessionID, Int16 result, Int16 lobbyNum)
        {
            var responseData = new PKTResLobbyEnter()
            {
                Result = result,
                LobbyNumber = lobbyNum
            };

            var packetData = MessagePack.MessagePackSerializer.Serialize(responseData);
            MsgPackPacketHeaderInfo.Write(packetData, 
                                    (UInt16)packetData.Length, 
                                    CSCommon.PacketID.ResLobbyEnter);

            SendNetworkFunc(netSessionID, packetData);
        }
    }
}
