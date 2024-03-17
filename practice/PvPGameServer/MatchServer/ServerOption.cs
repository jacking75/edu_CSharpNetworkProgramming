using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchServer
{
    public class ServerOption
    {
        public int ServerUniqueID { get; set; }

        public string Name { get; set; }
                
        public string RedisAddress { get; set; }
    }
}
