using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer;

public enum PROTOCOL_ID : UInt16
{
	BEGIN = 0,

	ECHO = 101,
            
    HEARTBEAT_START_NOTIFY = 3, // 서버에서 클라이언트로 허트 비트 시작을 알림
    HEARTBEAT_UPDATE_NOTIFY = 5, // 클라이언트에서 허버로 허트 비트 통보

    END
}


class PacketDef
{
    public const int HeaderSize = 5;
}

public class EchoPacket
{
    public string Data;

    public byte[] ToPacket(PROTOCOL_ID packetId, byte[] bodyData)
    {
        if (bodyData == null)
        {
            bodyData = Encoding.UTF8.GetBytes(Data);
        }

        var packetLen = (UInt16)(PacketDef.HeaderSize + bodyData.Length);
        var packet = new byte[packetLen];

        FreeNetLite.FastBinaryWrite.UInt16(packet, 0, packetLen);
        FreeNetLite.FastBinaryWrite.UInt16(packet, 2, (UInt16)packetId);
        Buffer.BlockCopy(bodyData, 0, packet, PacketDef.HeaderSize, bodyData.Length);
        return packet;
    }

    public void Decode(byte[] bodyData)
    {
        Data = Encoding.UTF8.GetString(bodyData);
    }
}


public class HeartBeatStartNtfPacket
{
    public UInt16 IntervalSec; // 허트비트 간격 (초)
    public byte[] ToPacket()
    {
        var packetLen = (UInt16)(PacketDef.HeaderSize + 2);
        var packet = new byte[packetLen];

        FreeNetLite.FastBinaryWrite.UInt16(packet, 0, packetLen);
        FreeNetLite.FastBinaryWrite.UInt16(packet, 2, (UInt16)PROTOCOL_ID.HEARTBEAT_START_NOTIFY);
        FreeNetLite.FastBinaryWrite.UInt16(packet, 5, IntervalSec);
        return packet;
    }

    public void Decode(byte[] bodyData)
    {
        IntervalSec = FreeNetLite.FastBinaryRead.UInt16(bodyData, 0);
    }
}
