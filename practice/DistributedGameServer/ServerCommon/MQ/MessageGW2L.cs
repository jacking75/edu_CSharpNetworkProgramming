using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.MQ
{
    [MessagePackObject]
    public class ReqLobbyEnter : PacketHead
    {
        [Key(1)]
        public string UserID;
        [Key(2)]
        public Int16 LobbyNumber;
    }

    [MessagePackObject]
    public class ResLobbyEnter : PacketHead
    {
        [Key(1)]
        public Int16 Result;
        [Key(2)]
        public Int16 LobbyNumber;
    }

  

    [MessagePackObject]
    public class ReqLobbyLeave : PacketHead
    {
        [Key(1)]
        public bool IsDisConnected;

        [Key(2)]
        public string userID;
    }

    [MessagePackObject]
    public class ResLobbyLeave : PacketHead
    {
        [Key(1)]
        public UInt16 Result;
    }
       
   
}
