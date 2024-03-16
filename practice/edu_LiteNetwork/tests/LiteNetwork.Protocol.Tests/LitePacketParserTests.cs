using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Protocol.Internal;
using LiteNetwork.Protocol.Tests.Processors;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace LiteNetwork.Protocol.Tests;

public sealed class LitePacketParserTests
{
    private readonly ILitePacketProcessor _packetProcessor;
    private LitePacketParser _packetParser;
    private readonly byte[] _buffer;
    private readonly byte[] _bufferHeader;
    private readonly byte[] _bufferContent;
    private readonly string _messageContent;

    private readonly byte[] _invalidBuffer;

    public LitePacketParserTests()
    {
        _packetProcessor = new LitePacketProcessor();
        _buffer = new List<int>(new[] { 16, 0, 0, 0, 12, 0, 0, 0, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33 }).Select(x => (byte)x).ToArray();
        _bufferHeader = _buffer.Take(_packetProcessor.HeaderSize).ToArray();
        _bufferContent = _buffer.Skip(_packetProcessor.HeaderSize).ToArray();
        _messageContent = Encoding.UTF8.GetString(_bufferContent.Skip(sizeof(int)).ToArray());

        _invalidBuffer = new List<int>(new[] { 255, 255, 255, 255, 12, 0, 0, 0, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33 }).Select(x => (byte)x).ToArray();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public void ParseIncomingDataTest(int bytesTransfered)
    {
        _packetParser = new LitePacketParser(_packetProcessor);
        var connection = new Mock<LiteConnection>();
        var token = new LiteDataToken(connection.Object);
        var numberOfReceivesNeeded = _buffer.Length / bytesTransfered + 1;
        var receviedMessages = new List<byte[]>();

        for (var i = 0; i < numberOfReceivesNeeded; i++)
        {
            var incomingBuffer = _buffer.Skip(i * bytesTransfered).Take(bytesTransfered).ToArray();

            IEnumerable<byte[]> messages = _packetParser.ParseIncomingData(token, incomingBuffer, Math.Min(bytesTransfered, incomingBuffer.Length));

            if (messages.Any())
            {
                receviedMessages.AddRange(messages);
            }
        }

        Assert.Single(receviedMessages);
        Assert.Equal(_messageContent, Encoding.UTF8.GetString(receviedMessages.Single().Skip(sizeof(int)).ToArray()));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public void ParseIncomingDataWithHeaderTest(int bytesTransfered)
    {
        _packetParser = new LitePacketParser(new DefaultLitePacketProcessor(includeHeader: true));

        var connection = new Mock<LiteConnection>();
        var token = new LiteDataToken(connection.Object);
        var numberOfReceivesNeeded = _buffer.Length / bytesTransfered + 1;
        var receviedMessages = new List<byte[]>();

        for (var i = 0; i < numberOfReceivesNeeded; i++)
        {
            var incomingBuffer = _buffer.Skip(i * bytesTransfered).Take(bytesTransfered).ToArray();

            IEnumerable<byte[]> messages = _packetParser.ParseIncomingData(token, incomingBuffer, Math.Min(bytesTransfered, incomingBuffer.Length));

            if (messages.Any())
            {
                receviedMessages.AddRange(messages);
            }
        }

        Assert.Single(receviedMessages);

        var messageContent = Encoding.UTF8.GetString(_buffer);
        Assert.Equal(messageContent, Encoding.UTF8.GetString(receviedMessages.Single().ToArray()));
    }

    [Fact]
    public void ParseIncomingDataWithInvalidSizeTest()
    {
        _packetParser = new LitePacketParser(_packetProcessor);
        var connection = new Mock<LiteConnection>();
        var token = new LiteDataToken(connection.Object);

        Assert.Throws<InvalidOperationException>(() => _packetParser.ParseIncomingData(token, _invalidBuffer, 32));
    }

}
