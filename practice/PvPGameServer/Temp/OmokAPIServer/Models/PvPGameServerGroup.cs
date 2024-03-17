using System;
using System.Collections.Generic;

namespace OmokAPIServer.Models
{
    public class PvPGameServerGroup
    {
        public List<GameServerInfo> ServerList { get; set; }
    }

    public class GameServerInfo
    {
        public Int32 Index { get; set; }
        public string IP { get; set; }
        public UInt16 Port { get; set; }
    }
}