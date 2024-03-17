namespace PvPGameServer.Enum
{
    public enum PacketID
    {
        // 시스템, 서버 - 서버
        NTF_IN_CONNECT_CLIENT = 21,
        NTF_IN_DISCONNECT_CLIENT = 22,
               
        NTF_IN_ROOM_LEAVE = 23,
        NTF_IN_ROOM_GAME_END = 26,
        NTF_IN_USERS_CHECK_STATE = 28,


        // 클라이언트 1001 ~ 2000
        CS_BEGIN        = 1001,

        REQ_LOGIN       = 1002,
        RES_LOGIN      = 1003,
        NTF_MUST_CLOSE       = 1005,

        REQ_ROOM_ENTER = 1015,
        RES_ROOM_ENTER = 1016,
        NTF_ROOM_USER_LIST = 1017,
        NTF_ROOM_NEW_USER = 1018,

        REQ_ROOM_LEAVE = 1021,
        RES_ROOM_LEAVE = 1022,
        NTF_ROOM_LEAVE_USER = 1023,

        REQ_ROOM_CHAT = 1026,
        RES_ROOM_CHAT = 1027,
        NTF_ROOM_CHAT = 1028,
        
        REQ_READY_OMOK = 1031,
        RES_READY_OMOK = 1032,
        NTF_READY_OMOK = 1033,
        NTF_START_OMOK = 1034,
        
        REQ_PUT_MOK = 1035,
        RES_PUT_MOK = 1036,
        NTF_PUT_MOK = 1037,
        
        NTF_END_OMOK = 1038,

        CS_END          = 1100,


        // Redis 답변 2001 ~ 2100
        REQ_REDIS_LOGIN_RESULT = 2001,
        NTF_REDIS_NEW_MATCHING_ROOM = 2006,
    }
}