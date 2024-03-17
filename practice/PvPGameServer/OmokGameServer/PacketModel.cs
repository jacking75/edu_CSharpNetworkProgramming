using MessagePack; //https://github.com/neuecc/MessagePack-CSharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PvPGameServer.Enum;
using ServerCommon;

namespace PvPGameServer
{
    public struct MsgPackPacketHeaderInfo
    {
        const int PacketHeaderMsgPackStartPos = 3;
        public const int HeadSize = 8;

        public UInt16 TotalSize;
        public UInt16 ID;
        public byte Type;

        public static UInt16 ReadTotalSize(byte[] data, int startPos)
        {
            return FastBinaryRead.UInt16(data, startPos + PacketHeaderMsgPackStartPos);
        }

        public static void WritePacketID(byte[] data, UInt16 packetId)
        {
            FastBinaryWrite.UInt16(data, PacketHeaderMsgPackStartPos + 2, packetId);
        }

        public static UInt16 ReadPacketID(byte[] data)
        {
            return FastBinaryRead.UInt16(data, PacketHeaderMsgPackStartPos + 2);
        }

        public void Read(byte[] headerData)
        {
            var pos = PacketHeaderMsgPackStartPos;

            TotalSize = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            ID = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Type = headerData[pos];
            pos += 1;
        }

        public void Write(byte[] packetData)
        {
            var pos = PacketHeaderMsgPackStartPos;

            FastBinaryWrite.UInt16(packetData, pos, TotalSize);
            pos += 2;

            FastBinaryWrite.UInt16(packetData, pos, ID);
            pos += 2;

            packetData[pos] = Type;
            pos += 1;
        }

        public static void Write(byte[] packetData, UInt16 totalSize, UInt16 id, byte type)
        {
            var pos = PacketHeaderMsgPackStartPos;

            FastBinaryWrite.UInt16(packetData, pos, totalSize);
            pos += 2;

            FastBinaryWrite.UInt16(packetData, pos, id);
            pos += 2;

            packetData[pos] = type;
            pos += 1;
        }
    }

    
 
    [MessagePackObject]
    public class MsgPackPacketHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[MsgPackPacketHeaderInfo.HeadSize];
    }
    
    [MessagePackObject]
    public class PKTResponse : MsgPackPacketHead
    {
        [Key(1)]
        public short Result;
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
    public class PKTResLogin : PKTResponse
    {
    }
        


    [MessagePackObject]
    public class PKTNtfMustClose : PKTResponse
    {
    }
    
    

    [MessagePackObject]
    public class PKTResRoomEnter : PKTResponse
    {
    }

    [MessagePackObject]
    public class PKTNtfRoomUserList : MsgPackPacketHead
    {
        [Key(1)]
        public List<string> UserIDList = new List<string>();
    }

    [MessagePackObject]
    public class PKTNtfRoomNewUser : MsgPackPacketHead
    {
        [Key(1)]
        public string UserID;
    }

    [MessagePackObject]
    public class PKTReqRoomLeave : MsgPackPacketHead
    {
    }

    [MessagePackObject]
    public class PKTResRoomLeave : PKTResponse
    {
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
    public class PKTResRoomChat : PKTResponse
    {
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
    public class PKTInternalReqRoomEnter : MsgPackPacketHead
    {
        [Key(1)]
        public int RoomNumber;

        [Key(2)]
        public string UserID;        
    }

    [MessagePackObject]
    public class PKTInternalResRoomEnter : PKTResponse
    {
        [Key(2)]
        public int RoomNumber;

        [Key(3)]
        public string UserID;
    }


    [MessagePackObject]
    public class PKTInternalNtfRoomLeave : MsgPackPacketHead
    {
        [Key(1)]
        public int RoomNumber;

        [Key(2)]
        public string UserID;
    }

    [MessagePackObject]
    public class PKTResReadyOmok : PKTResponse
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
    public class PKTReqPutMok : MsgPackPacketHead
    {
        [Key(1)] 
        public int PosX;
        [Key(2)]
        public int PosY;        
    }

    [MessagePackObject]
    public class PKTResPutMok : PKTResponse
    {
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
    public class PKTNtfEndOmok : MsgPackPacketHead
    {
        [Key(1)] 
        public string WinUserID;
    }

    [MessagePackObject]
    public class PKTInternalNtfRoomGameEnd : MsgPackPacketHead
    {
        [Key(1)]
        public bool IsFail;
        [Key(2)]
        public int RoomNumber;
        [Key(3)]
        public string WinUserID;
        [Key(4)]
        public string LoseUserID;
    }




    #region Redis To Packet Process
    [MessagePackObject]
    public class PKTResRedisLogin : MsgPackPacketHead
    {
        [Key(1)]
        public short Result;
        [Key(2)]
        public string UserID;
        [Key(3)]
        public int RoomNum;
    }


    [MessagePackObject]
    public class PKTNtfRedisMatchingRoom : MsgPackPacketHead
    {
        [Key(1)]
        public int RoomNum;
    }
    #endregion
}
