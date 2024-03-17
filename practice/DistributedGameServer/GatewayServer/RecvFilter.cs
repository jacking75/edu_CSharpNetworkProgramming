using MessagePack;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GatewayServer
{
    public struct PacketHeader
    {
        /*public const UInt16 PacketHeaderSize = 2 + 2 + 1;

        public UInt16    PacketSize;
        public UInt16    PacketID;
        public Byte     PacketType;*/
    }

    public class GWBinaryRequestInfo : BinaryRequestInfo
    {
        public string SessionID;
        public byte[] Data;

        public const int PacketHeaderMsgPackStartPos = 3;

        // PacketSize(UInt16) + PacketID(UInt16) + PacketType(Byte)
        public const int PacketHeaderSize = 5 + PacketHeaderMsgPackStartPos;

        public GWBinaryRequestInfo(byte[] packetData)
            : base(null, packetData)
        {
            Data = packetData;
        }
    }

    public class RecvFilter : FixedHeaderReceiveFilter<GWBinaryRequestInfo>
    {
        public RecvFilter() 
            : base(GWBinaryRequestInfo.PacketHeaderSize)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, offset, 2);

            var totalSize = BitConverter.ToUInt16(header, offset + GWBinaryRequestInfo.PacketHeaderMsgPackStartPos);
            return totalSize - GWBinaryRequestInfo.PacketHeaderSize;
        }

        protected override GWBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, GWBinaryRequestInfo.PacketHeaderSize);

            if (bodyBuffer != null)
            {
                var packetStartPos = offset - GWBinaryRequestInfo.PacketHeaderSize;
                var packetSize = length + GWBinaryRequestInfo.PacketHeaderSize;
                return new GWBinaryRequestInfo(bodyBuffer.CloneRange(packetStartPos, packetSize));
            }
            else
            {
                return new GWBinaryRequestInfo(header.Array);
            }
        }
    }
}
