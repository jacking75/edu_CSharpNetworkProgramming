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
        public void RequestLobbyChat(int mqIndex, PacketHeaderInfo mqHead, byte[] mqData)
        {
            try
            {
                Logger.ZLogDebug($"[RequestLobbyChat] Lobby Num: {mqHead.LobbyNumber}");

                var lobbyNumbr = mqHead.LobbyNumber;
                var gwServerIndex = mqHead.SenderIndex;
                var uid = mqHead.UID;

                var relayPacketData = new ReadOnlyMemory<byte>(mqData, 
                                        PacketHeaderInfo.HeadSize, 
                                        (mqData.Length - PacketHeaderInfo.HeadSize));
                var reqData = MessagePackSerializer.Deserialize<CSCommon.PKTReqLobbyChat>(relayPacketData);

                var lobby = GetLobby(lobbyNumbr);
                if (lobby == null)
                {
                    return;
                }

                var user = lobby.GetUser(uid);
                if (user == null)
                {
                    return;
                }


                lobby.NotifyPacketChatMsg(mqIndex, gwServerIndex, user.UID, user.UserID, reqData.Message);

                Logger.ZLogDebug("[RequestLobbyChat] Success");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex.ToString());
            }
        }
               
       
    }
}
