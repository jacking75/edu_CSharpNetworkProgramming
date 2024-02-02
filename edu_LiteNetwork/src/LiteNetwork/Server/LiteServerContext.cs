using LiteNetwork.Server.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace LiteNetwork.Server;

/// <summary>
/// Provides a server context.
/// </summary>
public sealed class LiteServerContext
{
    private readonly ILiteServer _server;

    /// <summary>
    /// Gets the server options.
    /// </summary>
    public LiteServerOptions Options => _server.Options;

    /// <summary>
    /// Gets the connected users.
    /// </summary>
    public IEnumerable<LiteConnection> Users => _server.Users;

    /// <summary>
    /// Creates a new <see cref="LiteServerContext"/>.
    /// </summary>
    /// <param name="server">Parent server.</param>
    internal LiteServerContext(ILiteServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Send a packet to the given <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="connection">Target connection.</param>
    /// <param name="packetBuffer">Packet buffer to send.</param>
    public void SendTo(LiteConnection connection, byte[] packetBuffer)
    {
        _server.SendTo(connection, packetBuffer);
    }

    /// <summary>
    /// Send a packet to a given collection of <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="connections">Collection of <see cref="LiteConnection"/>.</param>
    /// <param name="packetBuffer">Packet buffer to send.</param>
    public void SendTo(IEnumerable<LiteConnection> connections, byte[] packetBuffer)
    {
        _server.SendTo(connections, packetBuffer);
    }

    /// <summary>
    /// Send a packet to all connected users.
    /// </summary>
    /// <param name="packetBuffer">Packet buffer to send.</param>
    public void SendToAll(byte[] packetBuffer)
    {
        _server.SendToAll(packetBuffer);
    }

    /// <summary>
    /// Send a packet to all connected users except the given users.
    /// </summary>
    /// <param name="packetBuffer">Packet buffer to send.</param>
    /// <param name="excludedConnections">User connection to exclude from broadcast.</param>
    public void SendToAll(byte[] packetBuffer, params LiteConnection[] excludedConnections)
    {
        _server.SendTo(_server.Users.Except(excludedConnections), packetBuffer);
    }
}
