﻿using MessagePack; //https://github.com/neuecc/MessagePack-CSharp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_test_client
{    
    public struct MsgPackPacketHeadInfo
    {
        const int PacketHeaderMsgPackStartPos = 3;
        public const int HeadSize = 8;

        public UInt16 TotalSize;
        public UInt16 Id;
        public byte Type;

        public static UInt16 GetTotalSize(byte[] data, int startPos)
        {
            return FastBinaryRead.UInt16(data, startPos + PacketHeaderMsgPackStartPos);
        }
                
        public void Read(byte[] headerData)
        {
            var pos = PacketHeaderMsgPackStartPos;

            TotalSize = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Id = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Type = headerData[pos];
            pos += 1;
        }

        public void Write(byte[] mqData)
        {
            var pos = PacketHeaderMsgPackStartPos;

            FastBinaryWrite.UInt16(mqData, pos, TotalSize);
            pos += 2;

            FastBinaryWrite.UInt16(mqData, pos, Id);
            pos += 2;

            mqData[pos] = Type;
            pos += 1;
        }
    }


    [MessagePackObject]
    public class MsgPackPacketHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[MsgPackPacketHeadInfo.HeadSize];
    }

    // 로그인 요청
    [MessagePackObject]
    public class PKTReqLogin : MsgPackPacketHead
    {
        [Key(1)]
        public string UserID;
        [Key(2)]
        public string AuthToken;
    }

    [MessagePackObject]
    public class PKTResLogin : MsgPackPacketHead
    {
        [Key(1)]
        public short Result;
    }
    
    [MessagePackObject]
    public class PKTReqRoomEnter : MsgPackPacketHead
    {
        [Key(1)]
        public int RoomNumber;
    }

    [MessagePackObject]
    public class PKTResRoomEnter : MsgPackPacketHead
    {
        [Key(1)]
        public short Result;
    }
    
    [MessagePackObject]
    public class PKTNtfRoomUserList : MsgPackPacketHead
    {
        [Key(1)]
        public List<string> UserIDList = new List<string>();
    }

    [MessagePackObject]
    public class PKTReqRoomLeave : MsgPackPacketHead
    {
    }

    [MessagePackObject]
    public class PKTResRoomLeave : MsgPackPacketHead
    {
        [Key(1)]
        public short Result;
    }

    [MessagePackObject]
    public class PKTNtfRoomNewUser : MsgPackPacketHead
    {
        [Key(1)]
        public string UserID;
    }

    [MessagePackObject]
    public class PKTNtfRoomLeaveUser : MsgPackPacketHead
    {
        [Key(1)]
        public string UserID;
    }

    [MessagePackObject]
    public class PKTReqRoomChat : MsgPackPacketHead
    {
        [Key(1)]
        public string ChatMessage;
    }

    [MessagePackObject]
    public class PKTNtfRoomChat : MsgPackPacketHead
    {
        [Key(1)]
        public string UserID;

        [Key(2)]
        public string ChatMessage;
    }
    
    [MessagePackObject]
    public class PKTReqReadyOmok : MsgPackPacketHead
    {
    }
    
    [MessagePackObject]
    public class PKTNtfReadyOmok : MsgPackPacketHead
    {
        [Key(1)] 
        public string UserID;
        [Key(2)] 
        public bool IsReady;
    }
    [MessagePackObject]
    public class PKTNtfStartOmok : MsgPackPacketHead
    {
        [Key(1)] 
        public string FirstUserID; // 선턴 유저 ID
    }
    [MessagePackObject]
    public class PKTResPutMok : MsgPackPacketHead
    {
        [Key(1)] 
        public short Result; // 선턴 유저 ID
    }
    [MessagePackObject]
    public class PKTNtfPutMok : MsgPackPacketHead
    {
        [Key(1)] 
        public int PosX;
        [Key(2)]
        public int PosY;
        [Key(3)] 
        public int Mok;
    }
    
    [MessagePackObject]
    public class PKTReqPutMok : MsgPackPacketHead
    {
        [Key(1)] 
        public int PosX;
        [Key(2)]
        public int PosY;
        [Key(3)]
        public int Turn;
        [Key(4)] 
        public bool isSkip;
    }
    
    [MessagePackObject]
    public class PKTNtfEndOmok : MsgPackPacketHead
    {
        [Key(1)] 
        public string WinUserID;
    }





    #region Redis MessagePack Packet
    public struct MsgPackHeaderInfo
    {
        const int HeaderMsgPackStartPos = 3;
        public const int HeadSize = 5;

        public static void WriteID(byte[] data, UInt16 ID)
        {
            FastBinaryWrite.UInt16(data, HeaderMsgPackStartPos, ID);
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
    public class RequestPvPMatching : MsgPackHead
    {
        [Key(1)]
        public string UserID;

        [Key(2)]
        public Int32 RatingScore;
    }
    #endregion
}
