using CSCommon;
using ServerCommon;

using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace GatewayServer.CSPacket
{
    public partial class Handler
    {
        static readonly ILogger<Handler> Logger = ServerCommon.LogManager.GetLogger<Handler>();

        UInt16 ServerIndex = 0;

        ConnSession.Manager ConnSessionMgrRef;

        public static Func<string, byte[], bool> SendNetworkFunc;
        public static Action<Int16, byte[], Int32, Int32> SendMQToLobbyFunc;
        public static Action<Int32, byte[], Int32, Int32> SendMQToGameFunc;
        public static Action<byte[], Int32, Int32> SendMQToDBFunc;


        public void Init(UInt16 serverIndex, ConnSession.Manager connSessionMgr)
        {
            ServerIndex = serverIndex;
            ConnSessionMgrRef = connSessionMgr;
        }
               

        public void Process(GWBinaryRequestInfo reqPacket)
        {
            var packetID = MsgPackPacketHeaderInfo.ReadPacketID(reqPacket.Data);
            if (packetID <= PacketID.BEGIN || packetID >= PacketID.END)
            {
                Logger.ZLogError($"[Process] Invalid PacketID Range. SessioID:{reqPacket.SessionID}, packetID: {packetID}");
                return;
            }

            try
            {
                var session = ConnSessionMgrRef.GetSession(reqPacket.SessionID);
                var sessionState = (ConnSession.SessionState)session.CurState;

                if (sessionState == ConnSession.SessionState.NONE) // 로그인 미 완료
                {
                    ProcessStateConnected(session, packetID, reqPacket.Data);
                }
                else if (sessionState == ConnSession.SessionState.LOGIN) // ~ 로그인
                {
                    ProcessStateLogin(session, packetID, reqPacket.Data);
                }
                else if (sessionState == ConnSession.SessionState.LOBBY) // 로비 입장된 상태
                {
                    ProcessStateLobby(session, packetID, reqPacket.Data);
                }
                else // 게임 서버에 입장된 상태
                {
                    ProcessStateRoom(session, packetID, reqPacket.Data);
                }
            }
            catch(Exception ex)
            {
                Logger.ZLogError($"[Process] {ExceptionHelper.ExtractException(ex)}");
            }
            
        }

        public static void NotifyMustclosePacket(string sessionID, ErrorCode errCode)
        {
            var notifyData = new PKNtfMustClose()
            {
                Result = (Int16)errCode,
            };

            var packetData = MessagePack.MessagePackSerializer.Serialize(notifyData);
            MsgPackPacketHeaderInfo.Write(packetData, 
                (UInt16)packetData.Length, PacketID.NtfMustClose);
            SendNetworkFunc(sessionID, packetData);
        }

    }
}
