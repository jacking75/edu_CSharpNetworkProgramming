using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace LobbyServer.PKHandler
{
    public partial class Handler
    {
        static readonly ILogger<Handler> Logger = ServerCommon.LogManager.GetLogger<Handler>();

        public UInt16 ServerIndex;
        protected List<Lobby> LobbyListRef;
        protected Int16 StartLobbyNumber;

        const int MaxPacketLength = 8012;
        protected byte[] MQPacketEnCodeBuffer = new byte[MaxPacketLength];
        protected System.IO.MemoryStream MQPacketEnCodeStream { get; private set; }


        public void Init(UInt16 serverIndex, Int16 startLobbyNum, List<Lobby> lobbyList)
        {
            ServerIndex = serverIndex;
            StartLobbyNumber = startLobbyNum;
            LobbyListRef = lobbyList;
            
            MQPacketEnCodeStream = new System.IO.MemoryStream(MQPacketEnCodeBuffer);
        }


        Lobby GetLobby(int roomNumber)
        {
            var index = roomNumber - StartLobbyNumber - LobbyListRef[0].Number;
            if (index < 0 || index >= LobbyListRef.Count)
            {
                return null;
            }

            return LobbyListRef[index];
        }

        (Lobby, LobbyUser) GetLobbyAndLobbyUser(UInt16 lobbyNumber, UInt64 userUniqueId)
        {
            LobbyUser user = null;

            var lobby = GetLobby(lobbyNumber);
            if (lobby == null)
            {
                return (lobby, user);
            }

            user = lobby.GetUser(userUniqueId);
            return (lobby, user);
        }

        (bool, Lobby, LobbyUser) CheckLobbyAndLobbyUser(UInt16 lobbyNumber, UInt64 userUniqueId)
        {
            var roomObject = GetLobbyAndLobbyUser(lobbyNumber, userUniqueId);

            if (roomObject.Item1 == null || roomObject.Item2 == null)
            {
                return (false, roomObject.Item1, roomObject.Item2);
            }

            return (true, roomObject.Item1, roomObject.Item2);
        }

    }
}
