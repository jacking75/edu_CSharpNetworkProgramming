using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    public class LobbyUser
    {
        public string UserID { get; private set; }
        public UInt16 GatewayServerIndex { get; private set; }
        public UInt64 UID { get; private set; }

        public void Set(string userID, UInt16 gwServerIndex, UInt64 uinqueId)
        {
            UserID = userID;
            GatewayServerIndex = gwServerIndex;
            UID = uinqueId;
        }
    }
}
