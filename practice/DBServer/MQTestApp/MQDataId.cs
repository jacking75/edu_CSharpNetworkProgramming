using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{    
    public enum MqPacketId : UInt16
    {        
        // 디비 서버 관련
        MQ_REQ_GAME_RECORD = 2501,
        MQ_RES_GAME_RECORD = 2502,

        MQ_NTF_SAVE_GAME_RESULT = 2506,
    }
}
