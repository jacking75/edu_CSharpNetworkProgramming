﻿using LiteNetwork.Protocol;
using LiteNetwork.Protocol.Abstractions;

namespace LiteNetwork.Server;

/// <summary>
/// Builder options to use with host builder.
/// </summary>
public class LiteServerOptions
{
    /// <summary>
    /// Gets the default maximum of connections in accept queue.
    /// </summary>
    public const int DefaultBacklog = 50;

    /// <summary>
    /// Gets the default client buffer allocated size.
    /// </summary>
    public const int DefaultClientBufferSize = 128;

    /// <summary>
    /// Gets or sets the server's listening host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server's listening port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the maximum of pending connections queue.
    /// </summary>
    public int Backlog { get; set; } = DefaultBacklog;

    /// <summary>
    /// Gets or sets the handled client buffer size.
    /// </summary>
    public int ClientBufferSize { get; set; } = DefaultClientBufferSize;

    /// <summary>
    /// Gets or sets the receive strategy type.
    /// </summary>
    public ReceiveStrategyType ReceiveStrategy { get; set; }

    /// <summary>
    /// Gets the default server packet processor.
    /// </summary>
    public ILitePacketProcessor PacketProcessor { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="LiteServerOptions"/> instance
    /// with a default <see cref="LitePacketProcessor"/>.
    /// </summary>
    public LiteServerOptions()
    {
        PacketProcessor = new LitePacketProcessor();
    }
}
