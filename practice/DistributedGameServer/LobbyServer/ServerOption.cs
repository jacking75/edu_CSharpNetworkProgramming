using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public class ServerOption
    {
        public UInt16 ServerIndex { get; set; }

        public string Name { get; set; }

        public int ThreadCount { get; set; } = 0;

        public int LobbyCountPerThread { get; set; } = 0;

        public int LobbyMaxUserCount { get; set; } = 0;

        public int LobbyStartNumber { get; set; } = 0;                

        public List<string> MQServerAddressList { get; set; }
        
    }    
}
