using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.MQ
{
    [MessagePackObject]
    public class ReqGatewayLogin : PacketHead
    {
        [Key(1)]
        public string UserID;
        [Key(2)]
        public string AuthToken;
    }

    [MessagePackObject]
    public class ResGatewayLogin : PacketHead
    {
        [Key(1)]
        public Int16 Result;
        [Key(2)]
        public string UserID;
    }


    [MessagePackObject]
    public class NtfGatewayLogout : PacketHead
    {
        [Key(1)]
        public string UserID;
    }
}
