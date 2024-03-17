using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.Redis
{
    // API 서버에서 실시간 서버쪽 유저 인증토큰 넣은 정보
    public class UserAuthInfo
    {
        public string ID;
        public string AuthToken;
    }

    public class UserCurState
    {
        public Int32 LobbyNum;
        public Int32 RoomNum;
    }
}
