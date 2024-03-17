using ServerCommon;
using ServerCommon.MQ;

using MessagePack;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyServer.PKHandler
{
    public partial class Handler
    {
        public void RequestLobbyEnter(int mqIndex, PacketHeaderInfo mqHead, byte[] mqData)
        {
            try
            {
                var senderServerIndex = mqHead.SenderIndex;
                var userUniqueId = mqHead.UID;
                var reqData = MessagePackSerializer.Deserialize<ReqLobbyEnter>(mqData);

                Logger.ZLogDebug($"[ReqEnterLobby] Lobby Num: {reqData.LobbyNumber}");

                var lobby = GetLobby(reqData.LobbyNumber);
                if (lobby == null)
                {
                    SendResponseLobbyEnter(mqIndex, senderServerIndex, SErrorCode.EnterLobby_InvalidLobbyNumber, reqData.LobbyNumber, userUniqueId);
                    return;
                }

                LobbyEnter(mqIndex, mqHead, lobby, reqData);

                //TODO DB에 유저의 위치 상태 업데이트 통보

                Logger.ZLogDebug("[ReqEnterLobby] Success");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex.ToString());
            }
        }

        void LobbyEnter(int mqIndex, PacketHeaderInfo mqHead, Lobby lobby, ReqLobbyEnter reqData)
        {
            if (lobby.AddUser(reqData.UserID, mqHead.SenderIndex, mqHead.UID) == false)
            {
                SendResponseLobbyEnter(mqIndex, mqHead.SenderIndex, SErrorCode.EnterLobby_FailAdduser, reqData.LobbyNumber, mqHead.UID);
                return;
            }

            lobby.NotifyPacketNewUser(mqIndex, ServerIndex, mqHead.UID, reqData.UserID);

            SendResponseLobbyEnter(mqIndex, 
                                mqHead.SenderIndex, 
                                SErrorCode.None, 
                                reqData.LobbyNumber, 
                                mqHead.UID);

        }

        void SendResponseLobbyEnter(int mqIndex, UInt16 gwServerIndex, SErrorCode result, Int16 lobbyNum, UInt64 uid)
        {
            var subject = SubjectManager.ToGatewayServer(gwServerIndex);

            MQPacketEnCodeStream.Position = 0;

            var response = new ResLobbyEnter();
            response.Result = (Int16)result;
            response.LobbyNumber = lobbyNum;
            MessagePackSerializer.Serialize(MQPacketEnCodeStream, response);
            var sendDataSize = (int)MQPacketEnCodeStream.Position;

            var header = new PacketHeaderInfo();
            header.ID = PacketID.ResLobbyEnter;
            header.SenderIndex = ServerIndex;
            header.LobbyNumber = lobbyNum;
            header.UID = uid;
            header.Write(MQPacketEnCodeBuffer);

            Lobby.MQSendFunc(mqIndex, subject, MQPacketEnCodeBuffer, sendDataSize);
        }
    }
}
