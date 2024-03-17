using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace ServerCommon
{
    // 20000 ~ 
    public enum SErrorCode
    {
        None = 0,

        // Gateway 전용  20201 ~ 20400
        gwFailInitSetup = 20201,
        gwFailSuperSocketStart = 20202,

        // Lobby 20401 ~ 20600
        EnterLobby_InvalidLobbyNumber = 20421,
        EnterLobby_FailAdduser = 20422,

        LeaveLobby_InvalidLobbyNumber = 20426,
        LeaveLobby_InvalidUser = 20427,

        // Center  20601 ~ 20800

        // Match   20801 ~ 21000

        // Game 21001 ~ 21500 

        // DB  21501 ~ 21800
        dbGatewayLoginInvalidUser = 21501,
        dbGatewayLoginInvalidPW = 21502,
        dbGatewayLoginDuplicate = 21503,
    }
}
