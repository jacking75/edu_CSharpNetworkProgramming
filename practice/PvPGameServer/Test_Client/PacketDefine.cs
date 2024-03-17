using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_test_client
{
    public enum PACKET_ID : ushort
    {
        PACKET_ID_ECHO = 101,

        // Ping(Heart-beat)
        PACKET_ID_PING_REQ = 201,
        PACKET_ID_PING_RES = 202,

        PACKET_ID_ERROR_NTF = 203,


        REQ_LOGIN = 1002,
        RES_LOGIN = 1003,
        NTF_MUST_CLOSE = 1005,

        REQ_ROOM_ENTER = 1015,
        RES_ROOM_ENTER = 1016,
        NTF_ROOM_USER_LIST = 1017,
        NTF_ROOM_NEW_USER = 1018,

        REQ_ROOM_LEAVE = 1021,
        RES_ROOM_LEAVE = 1022,
        NTF_ROOM_LEAVE_USER = 1023,

        REQ_ROOM_CHAT = 1026,
        NTF_ROOM_CHAT = 1028,
        
        REQ_READY_OMOK = 1031,
        RES_READY_OMOK = 1032,
        NTF_READY_OMOK = 1033,
        NTF_START_OMOK = 1034,
        
        REQ_PUT_MOK = 1035,
        RES_PUT_MOK = 1036,
        NTF_PUT_MOK = 1037,

        NTF_END_OMOK = 1038,
    }


    public enum ErrorCode
    {
        None                        = 0, // 에러가 아니다

        // 서버 초기화 에라
        REDIS_INIT_FAIL             = 1,    // Redis 초기화 에러

        // 로그인 
        LOGIN_INVALID_AUTHTOKEN             = 1001, // 로그인 실패: 잘못된 인증 토큰
        ADD_USER_DUPLICATION                = 1002,
        LOGIN_INVALID_SESSION_ID = 1003,
        REMOVE_USER_SEARCH_FAILURE_USER_ID  = 1004,
        USER_AUTH_SEARCH_FAILURE_USER_ID    = 1005,
        USER_AUTH_ALREADY_SET_AUTH          = 1006,
        LOGIN_ALREADY_WORKING = 1007,
        LOGIN_FULL_USER_COUNT = 1008,
        LOGIN_NOT_EXIST_CLIENT_SESSION = 1009,
        LOGIN_NOT_FOUND_USER = 1010,

        DB_LOGIN_INVALID_PASSWORD   = 1011,
        DB_LOGIN_EMPTY_USER         = 1012,
        DB_LOGIN_EXCEPTION          = 1013,

        ROOM_INVALID_STATE = 1021,
        ROOM_INVALID_USER = 1022,
        ROOM_ERROR_SYSTEM = 1023,
        ROOM_INVALID_ROOM_NUMBER = 1024,
        ROOM_FAIL_ADD_USER = 1025,
        
        OMOK_OVERFLOW = 1031,
        OMOK_ALREADY_EXIST = 1032,
        OMOK_RENJURULE = 1033, // 쌍삼
        OMOK_TURN_NOT_MATCH = 1034,
        OMOK_NOT_STARTED = 1035,
    }
}
