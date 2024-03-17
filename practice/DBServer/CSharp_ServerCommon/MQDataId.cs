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


        MQ_REQ_LOAD_USER_GAME_DATA = 2515,
        MQ_RES_LOAD_USER_GAME_DATA = 2516,

        MQ_REQ_BUY_ITEM = 2601,
        MQ_RES_BUY_ITEM = 2602,
                
        MQ_REQ_CHANGE_QUICKSLOT = 2611,
        MQ_RES_CHANGE_QUICKSLOT = 2612,
                
    }
}
