namespace LiteNetwork.Protocol.Abstractions;

/// <summary>
/// Defines the behavior of a packet processor and how the packet should be handled.
/// </summary>
public interface ILitePacketProcessor
{
    /// <summary>
    /// Gets the packet header size that should contain the packet message size.
    /// </summary>
    int HeaderSize { get; }

    /// <summary>
    /// Gets a value that indicates whether to include the packet header in the final packet buffer.
    /// </summary>
    bool IncludeHeader { get; }

    /// <summary>
    /// Gets the packet message length with the given buffer.
    /// </summary>
    /// <param name="buffer">Header buffer.</param>
    /// <returns>Packet message data length.</returns>
    int GetMessageLength(byte[] buffer);

    /// <summary>
    /// Reads the packet header based on the given data token information.
    /// </summary>
    /// <param name="token">Current data token.</param>
    /// <param name="buffer">Current buffer from socket receive operation.</param>
    /// <param name="bytesTransfered">Number of bytes transfered by the socket.</param>
    /// <returns>True if the header is complete; otherwise, false.</returns>
    bool ReadHeader(LiteDataToken token, byte[] buffer, int bytesTransfered);

    /// <summary>
    /// Reads the packet content based on the given data token information.
    /// </summary>
    /// <param name="token">Current data token.</param>
    /// <param name="buffer">Current buffer from socket receive operation.</param>
    /// <param name="bytesTransfered">Number of bytes transfered by the socket.</param>
    void ReadContent(LiteDataToken token, byte[] buffer, int bytesTransfered);

    /// <summary>
    /// Append the packet header to the given buffer.
    /// </summary>
    /// <param name="buffer">Current packet buffer content.</param>
    /// <returns>The buffer with the packet header.</returns>
    byte[] AppendHeader(byte[] buffer);
}
