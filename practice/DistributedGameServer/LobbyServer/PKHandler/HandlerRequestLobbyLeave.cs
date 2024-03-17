using MessagePack;
using ServerCommon;
using ServerCommon.MQ;

using ZLogger;
using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyServer.PKHandler
{
    public partial class Handler
    {
        public void RequestLobbyLeave(int mqIndex, PacketHeaderInfo mqHead, byte[] mqData)
        {
            try
            {
                var lobbyNumbr = mqHead.LobbyNumber;
                var gwServerIndex = mqHead.SenderIndex;
                var uid = mqHead.UID;
                var reqData = MessagePackSerializer.Deserialize<ReqLobbyLeave>(mqData);

                Logger.ZLogDebug($"[RequestLobbyLeave] Lobby Num: {lobbyNumbr}");

                var lobby = GetLobby(lobbyNumbr);
                if (lobby == null)
                {
                    SendResponseLobbyLeave(mqIndex, gwServerIndex, SErrorCode.LeaveLobby_InvalidLobbyNumber, lobbyNumbr, uid);
                    return;
                }

                var user = lobby.GetUser(uid);
                if (user == null)
                {
                    SendResponseLobbyLeave(mqIndex, gwServerIndex, SErrorCode.LeaveLobby_InvalidUser, lobbyNumbr, uid);
                    return;
                }


                LobbyLeave(mqIndex, gwServerIndex, lobby, user, reqData.IsDisConnected);

                //TODO DB에 유저의 위치 상태 업데이트 통보

                Logger.ZLogDebug("[RequestLobbyLeave] - Success");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex.ToString());
            }
        }

        void LobbyLeave(int mqIndex, UInt16 gwServerIndex, Lobby lobby, LobbyUser user, bool isDisConnected)
        {
            lobby.RemoveUser(user.UID);

            lobby.NotifyPacketLeaveUser(mqIndex, ServerIndex, user.UID, user.UserID);

            if (isDisConnected == false)
            {
                SendResponseLobbyLeave(mqIndex, gwServerIndex, SErrorCode.None, lobby.Number, user.UID);
            }
        }

        void SendResponseLobbyLeave(int mqIndex, UInt16 gwServerIndex, SErrorCode result, Int16 lobbyNum, UInt64 uid)
        {
            var subject = SubjectManager.ToGatewayServer(gwServerIndex);
            MQPacketEnCodeStream.Position = 0;

            var response = new ResLobbyLeave();
            response.Result = (UInt16)result;
            MessagePackSerializer.Serialize(MQPacketEnCodeStream, response);
            var sendDataSize = (int)MQPacketEnCodeStream.Position;

            var header = new PacketHeaderInfo();
            header.ID = (UInt16)PacketID.ResLobbyLeave;
            header.Type = 0;
            header.SenderIndex = ServerIndex;
            header.LobbyNumber = lobbyNum;
            header.UID = uid;
            header.Write(MQPacketEnCodeBuffer);

            Lobby.MQSendFunc(mqIndex, subject, MQPacketEnCodeBuffer, sendDataSize);
        }
    }
}
