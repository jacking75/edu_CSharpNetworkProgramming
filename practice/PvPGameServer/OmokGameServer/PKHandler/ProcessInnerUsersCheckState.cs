using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.PKHandler
{
    public partial class Process
    {
        int StartCheckedUserIndex = 0;
        const int NumberOfUsersCheckedAtOnce = 100;

        void HandlerNtfInnerUsersCheckState(EFBinaryRequestInfo requestData)
        {
            var beginNum = StartCheckedUserIndex;
            var endNum = beginNum + NumberOfUsersCheckedAtOnce;

            if (endNum > UserMgr.MaxUserCount)
            {
                endNum = UserMgr.MaxUserCount;
                StartCheckedUserIndex = 0;
            }
            else
            {
                StartCheckedUserIndex = endNum;
            }

            var curTime = DateTime.Now;

            for (var index = beginNum; index < endNum; ++index)
            {
                var user = UserMgr.GetUserByIndex((int)index);
                if(user.OverReserveCloseNetworkTime(curTime))
                {
                    ForcedCloseSessionFunc(user.SessionID);
                }
            }

            //TODO 접속 이후 지정 시간까지 방에 들어가지 않고 있으면 정리하도록 한다
        }
    }
}
