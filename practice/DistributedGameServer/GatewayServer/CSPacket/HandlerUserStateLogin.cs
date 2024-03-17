using CSCommon;
using GatewayServer.ConnSession;
using ServerCommon.Redis;

using ZLogger;
using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;

namespace GatewayServer.CSPacket
{
    public partial class Handler
    {
        void ProcessStateLogin(Session session, UInt16 packetID, byte[] packet)
        {
            if (packetID == PacketID.ReqLobbyEnter)
            {
                var request = MessagePackSerializer.Deserialize<PKTReqLobbyEnter>(packet);
                ProcessLobbyEnter(session, request.LobbyNumber);
            }
            else
            {
                Logger.ZLogDebug($"[ProcessStateLogin] Invalid PacketID: {packetID}, NetSessionID: {session.NetSessionID}");
            }
        }

        ErrorCode ProcessLobbyEnter(Session session, Int16 lobbyNum)
        {
            if(session.SetReqLobbyEnter() == false)
            {
                SendResponseToClient(session.NetSessionID, (Int16)ErrorCode.LobbyEnterDisableEnter);
                return ErrorCode.LobbyEnterDisableEnter; 
            }

            
            var (len, mqPacket) = session.MakeRequestLobbyEnterMQPacket(ServerIndex, lobbyNum);
            SendMQToLobbyFunc(lobbyNum, mqPacket, 0, len);

            return ErrorCode.None;
        }

        void SendResponseToClient(string netSessionID, Int16 result)
        {
            var response = new PKTResLobbyEnter()
            {
                Result = result,
            };

            var packetData = MessagePackSerializer.Serialize(response);
            MsgPackPacketHeaderInfo.Write(packetData, (UInt16)packetData.Length, PacketID.ResLobbyEnter);
            SendNetworkFunc(netSessionID, packetData);
        }
    }
}
