using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.Redis
{
    public struct MsgPackDataHeaderInfo
    {
        const int HeaderStartPos = 3;
        public const int HeadSize = 5;

        public UInt16 TaskId;

        public static void WritePacketID(byte[] data, UInt16 taskId)
        {
            FastBinaryWrite.UInt16(data, HeaderStartPos, taskId);
        }

        public void Read(byte[] taskData)
        {
            var pos = HeaderStartPos;
            TaskId = FastBinaryRead.UInt16(taskData, pos);

        }

        /*public void Write(byte[] taskData)
        {
            var pos = HeaderStartPos;
            FastBinaryWrite.UInt16(taskData, pos, TaskId);
            
        }*/

        /*public static void Write(byte[] taskData, UInt16 taskId )
        {
            var pos = HeaderStartPos;
            FastBinaryWrite.UInt16(taskData, pos, taskId);
        }*/
    }


    [MessagePackObject]
    public class MsgPackTaskHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[MsgPackDataHeaderInfo.HeadSize];

        [Key(1)]
        public string NetSessionID;
    }

    public class ReqLoginTask : MsgPackTaskHead
    {
        [Key(2)]
        public string UserID;

        [Key(3)]
        public string AuthToken;
    }
}
