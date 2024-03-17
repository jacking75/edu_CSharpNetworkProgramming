using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.MQ
{
    [MessagePackObject]
    public class LobbyRangeMQInfo
    {
        [Key(0)]
        public Int32 StartNum { get; set; }
        [Key(1)]
        public Int32 LastNum { get; set; }
        [Key(2)]
        public Int16 ServerIndex { get; set; }
    }

    [MessagePackObject]
    public class RoomRangeMQInfo
    {
        [Key(0)]
        public Int32 StartNum { get; set; }
        [Key(1)]
        public Int32 LastNum { get; set; }
        [Key(2)]
        public Int16 ServerIndex { get; set; }
    }

    [MessagePackObject]
    public class ResLobbyRoomMQInfo : PacketHead
    {
        [Key(1)]
        public List<LobbyRangeMQInfo> LobbyList = new List<LobbyRangeMQInfo>();
        [Key(2)]
        public List<RoomRangeMQInfo> RoomList = new List<RoomRangeMQInfo>();
    }
}
