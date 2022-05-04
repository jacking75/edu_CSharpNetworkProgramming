using System;
using System.Collections.Generic;
using System.Text;

namespace FastSocketLite.Server.Messaging
{
    public class DefaultBinaryMessage : Messaging.IMessage
    {
        public UInt16 TotalSize;
        public UInt16 PacketID;
        public SByte Type;
        public UInt16 Version;
        public byte[] Body;

        public const Int16 HEADER_SIZE = sizeof(Int16) + sizeof(Int16) + sizeof(SByte) + sizeof(Int16);

        public DefaultBinaryMessage(UInt16 totalSize, UInt16 packetID, SByte type, UInt16 version, byte[] body)
        {
            TotalSize = totalSize;
            PacketID = packetID;
            Type = type;
            Version = version;
            Body = body;
        }
    }
}
