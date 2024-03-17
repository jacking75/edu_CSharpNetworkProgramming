using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.Redis
{
    [MessagePackObject]
    public class RequestPvPMatching : MsgPackHead
    {
        [Key(1)]
        public string UserID;

        [Key(2)]
        public Int32 RatingScore;
    }
    

    [MessagePackObject]
    public class UserPvPMatchingResult : MsgPackHead
    {
        [Key(1)]
        public List<string> UserList = new List<string>();
    }

    [MessagePackObject]
    public class NewMatchingRoom : MsgPackHead
    {
        [Key(1)]
        public int RoomNumber;
    }


    [MessagePackObject]
    public class PvPMatchingResult
    {
        [Key(0)]
        public string IP;
        [Key(1)]
        public UInt16 Port;
        [Key(2)]
        public Int32 RoomNumber;
        [Key(3)]
        public Int32 Index;
        [Key(4)]
        public string Token;
    }

    public class GameServerInfo
    {
        public Int32 Index;
        public string IP;
        public UInt16 Port;
    }
    public class PvPGameServerGroup
    {
        public List<GameServerInfo> ServerList;
    }
}
