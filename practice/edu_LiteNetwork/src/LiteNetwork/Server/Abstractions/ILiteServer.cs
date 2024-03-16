using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Server.Abstractions;

/// <summary>
/// Provides a basic abstraction to manage a TCP server.
/// </summary>
public interface ILiteServer : IDisposable
{
    /// <summary>
    /// Gets a boolean value that indicates if the server is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the server options.
    /// </summary>
    LiteServerOptions Options { get; }
    
    /// <summary>
    /// Gets all the connected users.
    /// </summary>
    IEnumerable<LiteConnection> Users { get; }

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns></returns>
    Task StartAsync();

    /// <summary>
    /// Starts the server with a cancellation token.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <returns></returns>
    Task StopAsync();

    /// <summary>
    /// Disconnects a user connection with the specified user id.
    /// </summary>
    /// <param name="userId">User id to disconnect.</param>
    void DisconnectUser(Guid userId);

    /// <summary>
    /// Disconnects the given <see cref="LiteConnection"/> instance.
    /// </summary>
    /// <param name="connection"></param>
    void DisconnectUser(LiteConnection connection);

    /// <summary>
    /// Send a packet to the given <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="connection">Target connection.</param>
    /// <param name="packet">Packet message to send.</param>
    void SendTo(LiteConnection connection, byte[] packet);

    /// <summary>
    /// Send a packet to a given collection of <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="connections">Collection of <see cref="LiteConnection"/>.</param>
    /// <param name="packet">Packet message to send.</param>
    void SendTo(IEnumerable<LiteConnection> connections, byte[] packet);

    /// <summary>
    /// Send a packet to all connected users.
    /// </summary>
    /// <param name="packet">Packet message to send.</param>
    void SendToAll(byte[] packet);
}
