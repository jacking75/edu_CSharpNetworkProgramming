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
        void ProcessStateRoom(Session session, UInt16 packetID, byte[] packet)
        {
            if (packetID == PacketID.ReqLobbyEnter)
            {
            }
            else
            {
                Logger.ZLogDebug($"[ProcessStateRoom] Invalid PacketID: {packetID}, NetSessionID: {session.NetSessionID}");
            }
        }
    }
}
