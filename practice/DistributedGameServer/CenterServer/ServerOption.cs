using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenterServer
{
    public class ServerOption
    {
        public UInt16 ServerIndex { get; set; }

        public string Name { get; set; }
        
        public List<string> MQServerAddressList { get; set; }
        
        public List<LobbyRangeMQInfo> LobbyRangeMQInfoList { get; set; }
        public List<RoomRangeMQInfo> RoomRangeMQInfoList { get; set; }
    }

    public class LobbyRangeMQInfo
    {
        public Int32 StartNum { get; set; }
        public Int32 LastNum { get; set; }
        public Int16 ServerIndex { get; set; }
    }

    public class RoomRangeMQInfo
    {
        public Int32 StartNum { get; set; }
        public Int32 LastNum { get; set; }
        public Int16 ServerIndex { get; set; }
    }
}
