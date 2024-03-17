using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer
{
    public class ServerOption
    {
        [Option( "serverIndex", Required = true, HelpText = "DB Server Index")]
        public UInt16 Index { get; set; }

        [Option("name", Required = true, HelpText = "Server Name")]
        public string Name { get; set; }

        [Option("threadCount", Required = true, HelpText = "Max Packet Thread Count")]
        public int ThreadCount { get; set; } = 0;

        [Option("mqServerAddress", Required = true, HelpText = "MQ Server Address")]
        public string MQServerAddress { get; set; }
                
        [Option("mySqlConnectionString", Required = true, HelpText = "mySqlConnectionString")]
        public string MySqlGameConnectionString { get; set; }                       
    }    
}
