using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{   
    public struct MQPacketHeadInfo
    {
        const int PACKET_HEADER_MSGPACK_START_POS = 3;
        public const int HeadSize = 14;

        public UInt16 Id;
        public UInt16 Type;
        public UInt16 SenderIndex;
        public UInt64 UserUniqueId;
        
        public void Read(byte[] headerData)
        {
            var pos = PACKET_HEADER_MSGPACK_START_POS;

            Id = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Type = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;
                        
            SenderIndex = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;
                        
            UserUniqueId = FastBinaryRead.UInt64(headerData, pos);
            pos += 8;
        }

        public void Write(byte[] mqData)
        {
            var pos = PACKET_HEADER_MSGPACK_START_POS;

            FastBinaryWrite.UInt16(mqData, pos, Id);
            pos += 2;

            FastBinaryWrite.UInt16(mqData, pos, Type);
            pos += 2;

            FastBinaryWrite.UInt16(mqData, pos, SenderIndex);
            pos += 2;

            FastBinaryWrite.UInt64(mqData, pos, UserUniqueId);
            pos += 8;
        }                
    }

   
    [MessagePackObject]
    public class MQPacketHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[MQPacketHeadInfo.HeadSize];
    }

  
    

    [MessagePackObject]
    public class MQReqGameRecord : MQPacketHead
    {
        [Key(1)]
        public UInt64 UserUID;
    }

    [MessagePackObject]
    public class MQResGameRecord : MQPacketHead
    {
        [Key(1)]
        public UInt64 WinCount;
        [Key(2)]
        public UInt64 DrawCount;
        [Key(3)]
        public UInt64 LoseCount;
    }

    [MessagePackObject]
    public class MQNTFSaveGameResult : MQPacketHead
    {
        [Key(1)]
        public UInt64 WinUserUId;
        [Key(2)]
        public UInt64 DefeatUserUId;
    }
     
}
