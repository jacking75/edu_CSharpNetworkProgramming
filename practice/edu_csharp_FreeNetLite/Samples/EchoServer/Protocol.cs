using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleServer
{
	public enum PROTOCOL_ID : UInt16
	{
		BEGIN = 0,

		ECHO_REQ = 1,
		ECHO_ACK = 2,
                
        HEARTBEAT_START_NOTIFY = 3, // 서버에서 클라이언트로 허트 비트 시작을 알림
        HEARTBEAT_UPDATE_NOTIFY = 5, // 클라이언트에서 허버로 허트 비트 통보

        END
	}

    public class PacketBase
    {
        public static readonly UInt16 HeaderSize = FreeNet.DefaultMessageResolver.HEADERSIZE;
    }

    public class EchoPacket : PacketBase
    {
        public string Data;

        public byte[] ToPacket(PROTOCOL_ID packetId, byte[] bodyData)
        {
            if (bodyData == null)
            {
                bodyData = Encoding.UTF8.GetBytes(Data);
            }

            var packetLen = (UInt16)(HeaderSize + bodyData.Length);
            var packet = new byte[packetLen];

            FreeNet.FastBinaryWrite.UInt16(packet, 0, packetLen);
            FreeNet.FastBinaryWrite.UInt16(packet, 2, (UInt16)packetId);
            Buffer.BlockCopy(bodyData, 0, packet, HeaderSize, bodyData.Length);
            return packet;
        }

        public void Decode(byte[] bodyData)
        {
            Data = Encoding.UTF8.GetString(bodyData);
        }
    }


    public class HeartBeatStartNtfPacket : PacketBase
    {
        public UInt16 IntervalSec; // 허트비트 간격 (초)
        public byte[] ToPacket()
        {
            var packetLen = (UInt16)(HeaderSize + 2);
            var packet = new byte[packetLen];

            FreeNet.FastBinaryWrite.UInt16(packet, 0, packetLen);
            FreeNet.FastBinaryWrite.UInt16(packet, 2, (UInt16)PROTOCOL_ID.HEARTBEAT_START_NOTIFY);
            FreeNet.FastBinaryWrite.UInt16(packet, 5, IntervalSec);
            return packet;
        }

        public void Decode(byte[] bodyData)
        {
            IntervalSec = FreeNet.FastBinaryRead.UInt16(bodyData, 0);
        }
    }
}
