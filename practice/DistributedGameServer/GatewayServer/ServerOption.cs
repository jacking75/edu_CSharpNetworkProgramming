using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayServer
{
    public class ServerOption
    {
        public UInt16 ServerUniqueID { get; set; }

        public string Name { get; set; }

        public int MaxConnectionNumber { get; set; }

        public int Port { get; set; }

        public int MaxRequestLength { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }
                
        public string MQServerAddress { get; set; }

     
    }
}
