using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon.MQ
{
    public struct PacketHeaderInfo
    {
        const int PACKET_HEADER_MSGPACK_START_POS = 3;
        const int PACKET_HEADER_MULTI_USER_DATA_OFFSET = PACKET_HEADER_MSGPACK_START_POS + 2 + 1 + 2 + 2 + 4 + 8;
        public const int HeadSize = 24;

        
        public UInt16 ID;
        public byte Type;
        public UInt16 SenderIndex;
        public Int16 LobbyNumber;
        public Int32 RoomNumber;
        public UInt64 UID;
        public UInt16 MultiUserDataOffset;

        public void Read(byte[] headerData)
        {
            var pos = PACKET_HEADER_MSGPACK_START_POS;

            ID = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            Type = headerData[pos];
            pos += 1;

            SenderIndex = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            LobbyNumber = FastBinaryRead.Int16(headerData, pos);
            pos += 2;

            RoomNumber = FastBinaryRead.Int32(headerData, pos);
            pos += 4;

            UID = FastBinaryRead.UInt64(headerData, pos);
            pos += 8;

            MultiUserDataOffset = FastBinaryRead.UInt16(headerData, pos);
        }

        public void Write(byte[] mqData, int offset = 0)
        {
            var pos = offset + PACKET_HEADER_MSGPACK_START_POS;

            FastBinaryWrite.UInt16(mqData, pos, ID);
            pos += 2;

            mqData[pos] = Type;
            pos += 1;

            FastBinaryWrite.UInt16(mqData, pos, SenderIndex);
            pos += 2;

            FastBinaryWrite.Int16(mqData, pos, LobbyNumber);
            pos += 2;

            FastBinaryWrite.Int32(mqData, pos, RoomNumber);
            pos += 4;

            FastBinaryWrite.UInt64(mqData, pos, UID);
            pos += 8;

            FastBinaryWrite.UInt16(mqData, pos, MultiUserDataOffset);
        }

        static public void WriteMultiUserDataOffset(byte[] mqData, UInt16 offset)
        {
            FastBinaryWrite.UInt16(mqData, PACKET_HEADER_MULTI_USER_DATA_OFFSET, offset);
        }
    }



    [MessagePackObject]
    public class PacketHead
    {
        [Key(0)]
        public Byte[] Head = new Byte[PacketHeaderInfo.HeadSize];
    }

  

    [MessagePackObject]
    public class GWUserUniqueIdList
    {
        [Key(0)]
        public UInt16 GWServerIndex;
        [Key(1)]
        public List<UInt64> UserUniqueIdList = new List<UInt64>();
    }
}
