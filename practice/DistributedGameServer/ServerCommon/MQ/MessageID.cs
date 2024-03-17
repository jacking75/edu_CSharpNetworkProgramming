using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.MQ
{
    // 1001 ~ 3000
    public class PacketID
    {
        // LOBBY
        public const UInt16 ReqLobbyEnter = 1021;
        public const UInt16 ResLobbyEnter = 1022;
       
        public const UInt16 ReqLobbyLeave = 1026;
        public const UInt16 ResLobbyLeave = 1027;
     
        public const UInt16 ReqLobbyRelay = 1036;
        public const UInt16 ResLobbyRelay = 1037;

       
        // Center Server  1901 ~ 2000
        public const UInt16 ReqLobbyRoomMqInfo = 1901;
        public const UInt16 ResLobbyRoomMqInfo = 1902;



        // 디비 서버 관련 2701 ~ 3000
        public const UInt16 ReqGatewayLogin = 2711;
        public const UInt16 ResGatewayLogin = 2712;
        public const UInt16 NtfGatewayLogout = 2716;
                
    }
}
