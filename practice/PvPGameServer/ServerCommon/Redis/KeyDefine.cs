using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.Redis
{
    public class KeyDefine
    {
        public const string RequestMatchingQueue = "req_matching";

        public const string PvPGameServerList = "pvp_gameServer_list";

        public const string PrefixGameServerRoomQueue = "gs_room_queue_";

        public const string PrefixMatchingResult = "ret_matching_";

        public const string PrefixMessageToGameServer = "$#MsgToGS_";
    }
}
