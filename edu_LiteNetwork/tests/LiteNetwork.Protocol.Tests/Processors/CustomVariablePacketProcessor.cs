using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteNetwork.Protocol.Tests.Processors;

/// <summary>
/// Custom packet processor with a header splitted into the following:
/// - [bytes] Integer with variable length (>= 1 byte and <= 5 bytes)
/// </summary>
public class CustomVariablePacketProcessor : LitePacketProcessor
{
    public override int GetMessageLength(byte[] buffer)
    {
        int length = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            length |= buffer[i];
        }

        return length;
    }

    public override bool ReadHeader(LiteDataToken token, byte[] buffer, int bytesTransfered)
    {
        int bufferRemainingBytes = bytesTransfered - token.DataStartOffset;

        if (bufferRemainingBytes <= 0)
        {
            return false;
        }

        var data = new List<byte>();
        int numRead = 0;
        byte read;

        do
        {
            read = buffer[token.DataStartOffset++];
            int value = (read & 0b01111111);

            data.Add((byte)(value << (7 * numRead)));

            numRead++;
            if (numRead > 5)
            {
                throw new InvalidOperationException("VarInt32 is too big.");
            }
        } while ((read & 0b10000000) != 0);

        token.HeaderData = token.HeaderData is null ? data.ToArray() : token.HeaderData.Concat(data).ToArray();

        return true;
    }
}
