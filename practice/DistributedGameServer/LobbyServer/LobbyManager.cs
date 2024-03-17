using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyServer
{
    public class LobbyManager
    {
        static readonly ILogger<LobbyManager> Logger = ServerCommon.LogManager.GetLogger<LobbyManager>();

        List<List<Lobby>> LobbyList = new List<List<Lobby>>();

        public void Init(ServerOption option)
        {
            Logger.ZLogInformation("[Init] - begin");

            var maxLobbyCount = option.ThreadCount * option.LobbyCountPerThread;
            
            for (int i = 0; i < option.ThreadCount; ++i)
            {
                LobbyList.Add(new List<Lobby>());
            }

            int lobbyIndex = -1;
            for (int i = 0; i < maxLobbyCount; ++i)
            {
                if (i == 0 || (i % option.LobbyCountPerThread) == 0)
                {
                    ++lobbyIndex;
                }

                var lobbyNumber = (option.LobbyStartNumber + i);
                var lobby = new Lobby();
                lobby.Init((UInt16)i, (Int16)lobbyNumber, (UInt16)option.LobbyMaxUserCount);

                LobbyList[lobbyIndex].Add(lobby);
            }

            Logger.ZLogInformation("[Init] - end");
        }

        public List<Lobby> GetLobbyList(int threadIndex)
        {
            return LobbyList[threadIndex];
        }

        public void LobbyRegisterRedis()
        {
            
        }
    }
}
