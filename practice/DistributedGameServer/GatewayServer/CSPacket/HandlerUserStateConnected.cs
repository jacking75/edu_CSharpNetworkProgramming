using CSCommon;
using GatewayServer.ConnSession;

using ZLogger;
using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;

namespace GatewayServer.CSPacket
{
    public partial class Handler
    {
        void ProcessStateConnected(Session session, UInt16 packetID, byte[] packet)
        {
            if (packetID == PacketID.ReqLogin)
            {
                var request = MessagePackSerializer.Deserialize<PKTReqLogin>(packet);
                RequestDBLogin(session, request.UserID, request.AuthToken);
            }
            else
            {
                Logger.ZLogDebug($"[ProcessStateConnected] Invalid PacketID: {packetID}, NetSessionID: {session.NetSessionID}");
            }
        }

        void RequestDBLogin(Session session, string userID, string authToken)
        {
            if(session.SetReqLobbyEnter() == false)
            {
                Logger.ZLogDebug($"[RequestDBLogin] Fail SetReqLobbyEnter. NetSessionID: {session.NetSessionID}");
                return;
            }

            var (len, mqPacket) = session.MakeRequestGatewayLoginMQPacket(ServerIndex, userID, authToken);
            SendMQToDBFunc(mqPacket, 0, len);
        }
    }
}
