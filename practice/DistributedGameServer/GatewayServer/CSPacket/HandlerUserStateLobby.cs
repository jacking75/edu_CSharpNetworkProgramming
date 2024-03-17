using CSCommon;
using GatewayServer.ConnSession;

using ZLogger;

using System;
using System.Collections.Generic;
using System.Text;

namespace GatewayServer.CSPacket
{
    public partial class Handler
    {
        void ProcessStateLobby(Session session, UInt16 packetID, byte[] packet)
        {
            if (packetID == PacketID.ReqLobbyLeave)
            {
                var (len, mqPacket) = session.MakeRequestLobbyLeaveMQPacket(ServerIndex, false);
                SendMQToLobbyFunc(session.LobbyNum, mqPacket, 0, len);
            } 
            else if(packetID > PacketID.RelayLobbyBegin && packetID < PacketID.RelayLobbyEnd)
            {
                var (len, mqPacket) = session.MakeRequestLobbyRelayMQPacket(ServerIndex, packet);
                SendMQToLobbyFunc(session.LobbyNum, mqPacket, 0, len);
            }
            else
            {
                Logger.ZLogDebug($"[ProcessStateLobby] Invalid PacketID: {packetID}, NetSessionID: {session.NetSessionID}");
            }
        }
    }
}
