using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class LogEventID
    {
        // 게이트웨이
        // 로비 서버
        // 게임 서버
        // 매치 서버
        // 센터 서버
        // DB 서버 7001
        public const int DBProgramInit = 7001;
        public const int DBProgramEnd = 7002;
        public const int DBLogicErrorProcessWorkerQueue = 7003;
    }
}
