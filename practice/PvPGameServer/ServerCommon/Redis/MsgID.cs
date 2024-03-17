using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Redis
{
    public enum MsgID
    {
        ReloadGameServerInfo = 1,
        RequestMatching = 2,
        ResponseMatching = 3,
        CancelMatching = 4,
        NewMatcingRoom = 5,


        ReqLogin = 101,
        NtfRegistAvailableRoom = 102,
    }
}
