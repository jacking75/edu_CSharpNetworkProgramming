using Microsoft.Extensions.Logging;
using ZLogger;

using MessagePack;
using ServerCommon;
using ServerCommon.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer.PKHandler
{
    public class GatewayLogout : Base
    {
        public static readonly ILogger<GatewayLogout> Logger = LogManager.GetLogger<GatewayLogout>();

        public GatewayLogout(DBMysql sql, DBRedis redis)
        {
            SQLDB = sql;
            RedisDB = redis;
        }

        public override void Process(PacketDataParams para)
        {
            try
            {
                ProcessImpl(para);
                Logger.ZLogDebug("[GatewayLogout] - Success");
            }
            catch (Exception ex)
            {
                Logger.ZLogError(ex, "");
            }
        }

        void ProcessImpl(PacketDataParams para)
        {
            var ntfData = MessagePackSerializer.Deserialize<NtfGatewayLogout>(para.MQData);
            RedisDB.GatewayLogout(ntfData.UserID);
        }
    }
}
