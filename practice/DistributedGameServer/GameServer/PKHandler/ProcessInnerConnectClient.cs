using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.PKHandler
{
    public partial class Process
    {
        void HandlerNtfInnerConnectClient(EFBinaryRequestInfo requestData)
        {
            MainServer.GlobalLogger.Debug($"접속 완료: SessionID:{requestData.SessionID}");

            var result = UserMgr.AddUser(requestData.SessionID);

            if(result != Enum.ErrorCode.None)
            {
                ResponseToClient<PKTNtfMustClose>(Enum.PacketID.NTF_MUST_CLOSE, result, requestData.SessionID);
            }
        }
    }
}
