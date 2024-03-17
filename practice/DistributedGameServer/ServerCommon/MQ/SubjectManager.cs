using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.MQ
{
    public class SubjectManager
    {
        public static string ToCenterServer()
        {
            return $"to.C";
        }

        public static string ToDBServer()
        {
            return $"to.DB";
        }

        public static string ToLobbyServer(UInt16 serverIndex)
        {
            return $"to.L{serverIndex}";
        }

        public static string ToGatewayServer(UInt16 serverIndex)
        {
            return $"to.G{serverIndex}";
        }
    }
}
