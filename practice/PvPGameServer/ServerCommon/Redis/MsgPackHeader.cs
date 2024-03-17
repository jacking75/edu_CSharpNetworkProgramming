using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon.Redis
{
    public struct MsgPackHeaderInfo
    {
        const int HeaderMsgPackStartPos = 3;
        public const int HeadSize = 5;

        public static void WriteID(byte[] data, MsgID ID)
        {
            FastBinaryWrite.UInt16(data, HeaderMsgPackStartPos, (UInt16)ID);
        }

        public static UInt16 ReadID(byte[] data)
        {
            return FastBinaryRead.UInt16(data, HeaderMsgPackStartPos);
        }
    }

    [MessagePackObject]
    public class MsgPackHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[MsgPackHeaderInfo.HeadSize];
    }
}
