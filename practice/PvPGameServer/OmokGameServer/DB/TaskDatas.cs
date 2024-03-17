using MessagePack;
using ServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.DB
{
    public struct MsgPackHeaderInfo
    {
        const int HeaderMsgPackStartPos = 3;
        public const int HeadSize = 5;

        public static void WriteID(byte[] data, TaskID ID)
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



    [MessagePackObject]
    public class NtfSaveGameResultTask : MsgPackHead
    {
        [Key(1)]
        public string WinUserID;

        [Key(2)]
        public string LoseUserID;
    }
}
