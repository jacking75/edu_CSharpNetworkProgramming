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

