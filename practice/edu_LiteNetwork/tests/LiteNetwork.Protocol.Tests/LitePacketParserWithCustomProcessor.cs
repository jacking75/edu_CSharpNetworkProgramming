﻿using LiteNetwork.Protocol.Internal;
using LiteNetwork.Protocol.Tests.Processors;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LiteNetwork.Protocol.Tests;

public class LitePacketParserWithCustomProcessor
{
    public static IEnumerable<object[]> Packets = new List<object[]>
    {
        new object[]
        {
            2, // Bytes transfered
            1, // Header length
            4, // Packet length
            // Packet data
            new byte[] { 0x04, 0x02, 0x03, 0x05, 0x03 }
        },
        new object[]
        {
            4, // Bytes transfered
            1, // Header length
            4, // Packet length
            // Packet data
            new byte[] { 0x04, 0x02, 0x03, 0x05, 0x03 }
        },
        new object[]
        {
            8, // Bytes transfered
            1, // Header length
            4, // Packet length
            // Packet data
            new byte[] { 0x04, 0x02, 0x03, 0x05, 0x03 }
        },
        new object[]
        {
            2, // Bytes transfered
            2, // Header length
            128, // Packet length
            // Packet data
            new byte[]
            {
                // Header = 128
                0x80, 0x01,
                // Data
                0x03,0x98,0xFF,0x29,0xCC,0x80,0xD8,0x20,0x71,0xC7,0x35,0xF7,0x5A,0xEA,0x7D,0x7A,0xB8,0xBB,0xF9,0xA3,0xFE,0xCC,0x81,0xAD,0xB3,0xC0,0x38,0x9D,0xED,0x66,0xE8,0x48,0xB1,0xCF,0xD9,0x04,0xFD,0x7D,0x7E,0x5B,0x05,0xE1,0x58,0x76,0xBB,0xF3,0x4B,0x76,0x04,0x98,0xC7,0xAC,0xB3,0x66,0xEF,0x37,0x16,0x52,0x75,0x0C,0x47,0x3A,0x33,0x0B,0xDF,0xEC,0x45,0x8A,0x10,0x78,0x7D,0x3B,0x3C,0x9E,0x9E,0x1D,0x74,0x0B,0xF2,0xC0,0x75,0xC1,0x99,0x55,0x9F,0x3C,0x95,0x4B,0xDF,0xA2,0xEE,0xAA,0x07,0x72,0x13,0xCA,0xAE,0x4D,0x97,0xCE,0xAD,0xC0,0x66,0x8C,0x1A,0x8B,0x6F,0x15,0xC8,0xD2,0xC3,0x0B,0x60,0xB4,0x97,0x86,0xA0,0xDD,0x6B,0xA2,0x57,0xFA,0xAB,0x6E,0x8A,0xD3,0x33,0xCA
            }
        },
        new object[]
        {
            32, // Bytes transfered
            2, // Header length
            128, // Packet length
            // Packet data
            new byte[]
            {
                // Header = 128
                0x80, 0x01,
                // Data
                0x03,0x98,0xFF,0x29,0xCC,0x80,0xD8,0x20,0x71,0xC7,0x35,0xF7,0x5A,0xEA,0x7D,0x7A,0xB8,0xBB,0xF9,0xA3,0xFE,0xCC,0x81,0xAD,0xB3,0xC0,0x38,0x9D,0xED,0x66,0xE8,0x48,0xB1,0xCF,0xD9,0x04,0xFD,0x7D,0x7E,0x5B,0x05,0xE1,0x58,0x76,0xBB,0xF3,0x4B,0x76,0x04,0x98,0xC7,0xAC,0xB3,0x66,0xEF,0x37,0x16,0x52,0x75,0x0C,0x47,0x3A,0x33,0x0B,0xDF,0xEC,0x45,0x8A,0x10,0x78,0x7D,0x3B,0x3C,0x9E,0x9E,0x1D,0x74,0x0B,0xF2,0xC0,0x75,0xC1,0x99,0x55,0x9F,0x3C,0x95,0x4B,0xDF,0xA2,0xEE,0xAA,0x07,0x72,0x13,0xCA,0xAE,0x4D,0x97,0xCE,0xAD,0xC0,0x66,0x8C,0x1A,0x8B,0x6F,0x15,0xC8,0xD2,0xC3,0x0B,0x60,0xB4,0x97,0x86,0xA0,0xDD,0x6B,0xA2,0x57,0xFA,0xAB,0x6E,0x8A,0xD3,0x33,0xCA
            }
        },
        new object[]
        {
            64, // Bytes transfered
            2, // Header length
            128, // Packet length
            // Packet data
            new byte[]
            {
                // Header = 128
                0x80, 0x01,
                // Data
                0x03,0x98,0xFF,0x29,0xCC,0x80,0xD8,0x20,0x71,0xC7,0x35,0xF7,0x5A,0xEA,0x7D,0x7A,0xB8,0xBB,0xF9,0xA3,0xFE,0xCC,0x81,0xAD,0xB3,0xC0,0x38,0x9D,0xED,0x66,0xE8,0x48,0xB1,0xCF,0xD9,0x04,0xFD,0x7D,0x7E,0x5B,0x05,0xE1,0x58,0x76,0xBB,0xF3,0x4B,0x76,0x04,0x98,0xC7,0xAC,0xB3,0x66,0xEF,0x37,0x16,0x52,0x75,0x0C,0x47,0x3A,0x33,0x0B,0xDF,0xEC,0x45,0x8A,0x10,0x78,0x7D,0x3B,0x3C,0x9E,0x9E,0x1D,0x74,0x0B,0xF2,0xC0,0x75,0xC1,0x99,0x55,0x9F,0x3C,0x95,0x4B,0xDF,0xA2,0xEE,0xAA,0x07,0x72,0x13,0xCA,0xAE,0x4D,0x97,0xCE,0xAD,0xC0,0x66,0x8C,0x1A,0x8B,0x6F,0x15,0xC8,0xD2,0xC3,0x0B,0x60,0xB4,0x97,0x86,0xA0,0xDD,0x6B,0xA2,0x57,0xFA,0xAB,0x6E,0x8A,0xD3,0x33,0xCA
            }
        }
    };

    private readonly LiteDataToken _token;
    private readonly LitePacketParser _packetParser;

    public LitePacketParserWithCustomProcessor()
    {
        var connection = new Mock<LiteConnection>();
        _token = new LiteDataToken(connection.Object);
        _packetParser = new LitePacketParser(new CustomVariablePacketProcessor());
    }

    [Theory]
    [MemberData(nameof(Packets))]
    public void ParseIncomingDataUsingCustomProcessor(int bytesTransfered, int headerLength, int packetLength, byte[] packetData)
    {
        var numberOfReceivesNeeded = packetData.Length / bytesTransfered + 1;
        var receivedMessages = new List<byte[]>();

        for (var i = 0; i < numberOfReceivesNeeded; i++)
        {
            var incomingBuffer = packetData.Skip(i * bytesTransfered).Take(bytesTransfered).ToArray();

            IEnumerable<byte[]> messages = _packetParser.ParseIncomingData(_token, incomingBuffer, Math.Min(bytesTransfered, incomingBuffer.Length));

            if (messages.Any())
            {
                receivedMessages.AddRange(messages);
            }
        }

        Assert.Single(receivedMessages);

        byte[] receivedMessage = receivedMessages.First();

        Assert.Equal(packetLength, receivedMessage.Length);
        Assert.Equal(packetData.Skip(headerLength).ToArray(), receivedMessage);
    }
}
