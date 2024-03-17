using System;
using MessagePack;
using OmokAPIServer.Helper;

namespace OmokAPIServer.Models.Redis
{
    public enum TaskId
    {
        ReloadGameServerInfo = 1,
        RequestMatching = 2,
        ResponseMatching = 3,
        CancelMatching = 4,
    }
    
    public struct MsgPackHeaderInfo
    {
        const int HeaderMsgPackStartPos = 3;
        public const int HeadSize = 5;
        //public UInt16 Id;
        
        public static void WriteId(byte[] data, UInt16 Id)
        {
            FastBinaryWrite.UInt16(data, HeaderMsgPackStartPos, Id);
        }

        public static UInt16 ReadId(byte[] data)
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
    public class RequestPvPMatching : MsgPackHead
    {
        [Key(1)]
        public string UserID;

        [Key(2)]
        public Int32 RatingScore;
    }

    [MessagePackObject]
    public class PvPMatchingResult
    {
        [Key(0)]
        public string IP;
        [Key(1)]
        public UInt16 Port;
        [Key(2)]
        public Int32 RoomNumber;
        [Key(3)]
        public Int32 Index;
        [Key(4)]
        public string Token;
    }
}