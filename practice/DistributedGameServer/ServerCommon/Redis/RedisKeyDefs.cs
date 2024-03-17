using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.Redis
{
    public class RedisKeyDefs
    {
        public const string GatewayUserAuthPrefix = "gwAuth_"; // + 유저아이디
        public const string UserCurStatePrefix = "userState_"; // + 유저아이디
    }
}
