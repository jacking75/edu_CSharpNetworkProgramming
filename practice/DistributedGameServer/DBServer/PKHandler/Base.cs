using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer.PKHandler
{
    public class Base
    {
        public static Action<Int32, string, byte[], int> MQSendFunc;

        protected DBMysql SQLDB = null;
        protected DBRedis RedisDB = null;

       
        public virtual void Process(PacketDataParams pkParams) { }
    }

    public class PacketDataParams
    {
        public UInt16 MyServerIndex;
        public byte[] EncodingBuffer;
        public MemoryStream EncodingStream;
        public int MQIndex;
        public ServerCommon.MQ.PacketHeaderInfo MQHeader;
        public byte[] MQData;
    }
}
