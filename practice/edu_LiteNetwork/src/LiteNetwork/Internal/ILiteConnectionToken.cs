using LiteNetwork.Protocol;
using System;
using System.Collections.Generic;

namespace LiteNetwork.Internal;

/// <summary>
/// Provides a structure to used in receiver process.
/// </summary>
internal interface ILiteConnectionToken : IDisposable
{
    /// <summary>
    /// Gets the connection attached to the current token.
    /// </summary>
    LiteConnection Connection { get; }

    /// <summary>
    /// Gets the data token.
    /// </summary>
    LiteDataToken DataToken { get; }

    /// <summary>
    /// Process a received messages.
    /// </summary>
    /// <param name="messages">Collection of message data buffers.</param>
    void ProcessReceivedMessages(IEnumerable<byte[]> messages);
}
