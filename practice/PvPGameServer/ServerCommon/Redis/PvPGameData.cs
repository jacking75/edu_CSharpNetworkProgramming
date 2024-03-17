using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Redis
{
    [MessagePackObject]
    public class ReqLoginTask : MsgPackHead
    {
        [Key(1)]
        public string NetSessionID;

        [Key(2)]
        public string UserID;

        [Key(3)]
        public string AuthToken;
    }


    [MessagePackObject]
    public class NtfRegistAvailableRoomTask : MsgPackHead
    {
        [Key(1)]
        public int RoomNumber;
    }
}
