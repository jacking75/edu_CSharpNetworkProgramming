using System;

namespace LiteNetwork.Protocol;

/// <summary>
/// Provides a data structure that defines a lite packet data.
/// </summary>
public class LiteDataToken
{
    /// <summary>
    /// Gets or sets the message header data.
    /// </summary>
    public byte[]? HeaderData { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes received for the the message header.
    /// </summary>
    public int ReceivedHeaderBytesCount { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates if the header is complete.
    /// </summary>
    public bool IsHeaderComplete { get; set; }

    /// <summary>
    /// Gets or sets the full message size.
    /// </summary>
    public int? MessageSize { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes received for the message body.
    /// </summary>
    public int ReceivedMessageBytesCount { get; set; }

    /// <summary>
    /// Gets or sets the received message data.
    /// </summary>
    public byte[]? MessageData { get; set; }

    /// <summary>
    /// Gets or sets the data start offset.
    /// </summary>
    public int DataStartOffset { get; set; }

    /// <summary>
    /// Gets a value that indicates if the message is complete.
    /// </summary>
    public bool IsMessageComplete => MessageSize.HasValue && ReceivedMessageBytesCount == MessageSize.Value;

    /// <summary>
    /// Gets the connection attached to the current data token.
    /// </summary>
    public LiteConnection Connection { get; }

    /// <summary>
    /// Creates a new <see cref="LiteDataToken"/> instance.
    /// </summary>
    /// <param name="connection">Current connection attached to this data token.</param>
    /// <exception cref="ArgumentNullException">Thrown when the given connection is null.</exception>
    public LiteDataToken(LiteConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Reset the token data properties.
    /// </summary>
    internal void Reset()
    {
        IsHeaderComplete = false;
        ReceivedHeaderBytesCount = 0;
        ReceivedMessageBytesCount = 0;
        HeaderData = null;
        MessageData = null;
        MessageSize = null;
    }
}
