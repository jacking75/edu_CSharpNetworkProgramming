using LiteNetwork.Exceptions;
using LiteNetwork.Internal;
using LiteNetwork.Server.Abstractions;
using LiteNetwork.Server.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Server;

/// <summary>
/// Provides a basic TCP server implementation handling users of type <typeparamref name="TUser"/>.
/// </summary>
/// <typeparam name="TUser">The user type that the server will handle.</typeparam>
public class LiteServer<TUser> : ILiteServer
    where TUser : LiteServerUser
{
    private readonly ILogger<LiteServer<TUser>>? _logger;
    private readonly IServiceProvider? _serviceProvider;
    private readonly ConcurrentDictionary<Guid, TUser> _connectedUsers;
    private readonly Socket _socket;
    private readonly LiteServerAcceptor _acceptor;
    private readonly LiteServerReceiver _receiver;

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }

    /// <inheritdoc/>
    public LiteServerOptions Options { get; }

    /// <inheritdoc/>
    public IEnumerable<LiteConnection> Users => _connectedUsers.Values;

    /// <summary>
    /// Creates a new <see cref="LiteServer{TUser}"/> instance with a server configuration 
    /// and a service provider.
    /// </summary>
    /// <param name="options">Server configuration options.</param>
    /// <param name="serviceProvider">Service provider to use.</param>
    public LiteServer(LiteServerOptions options, IServiceProvider? serviceProvider = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options;
        _serviceProvider = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
        _logger = _serviceProvider.GetService<ILogger<LiteServer<TUser>>>();

        _connectedUsers = new ConcurrentDictionary<Guid, TUser>();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

        _acceptor = new LiteServerAcceptor(_socket);
        _acceptor.OnClientAccepted += OnClientAccepted;
        _acceptor.OnError += OnAcceptorError;

        _receiver = new LiteServerReceiver(options.PacketProcessor, Options.ReceiveStrategy, Options.ClientBufferSize);
        _receiver.Disconnected += OnDisconnected;
        _receiver.Error += OnReceiverError;
    }

    /// <summary>
    /// Gets a connected <typeparamref name="TUser"/> associated with the specified id.
    /// </summary>
    /// <param name="userId">User id to get.</param>
    /// <returns>A <typeparamref name="TUser"/> with the specified id if the id has found;
    /// otherwise, null.</returns>
    public TUser? GetUser(Guid userId) => TryGetUser(userId, out TUser? user) ? user : default;

    /// <summary>
    /// Attempts to get the <typeparamref name="TUser"/> associated with the specified id.
    /// </summary>
    /// <param name="userId">User id to get.</param>
    /// <param name="user">If the operation completed returns the user associated with the specified id,
    /// or null if the operaton failed.
    /// </param>
    /// <returns>True if the user id has found; otherwise, false.</returns>
    public bool TryGetUser(Guid userId, out TUser? user) => _connectedUsers.TryGetValue(userId, out user);

    /// <summary>
    /// Starts to listening and accept users asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the <see cref="LiteServer{TUser}"/> starts.</returns>
    public async Task StartAsync() => await StartAsync(CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Starts to listening and accept users asynchronously with the specified <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">Used to indicate when stop should no longer be successfully.</param>
    /// <returns>A <see cref="Task"/> that completes when the <see cref="LiteServer{TUser}"/> starts.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Server is already running.");
        }

        OnBeforeStart();

        IPEndPoint localEndPoint = await LiteNetworkHelpers.CreateIpEndPointAsync(Options.Host, Options.Port).ConfigureAwait(false);
        _socket.Bind(localEndPoint);
        _socket.Listen(Options.Backlog);
        _acceptor.StartAccept();
        IsRunning = true;

        OnAfterStart();
    }

    /// <summary>
    /// Attempt to stop the server asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the <see cref="LiteServer{TUser}"/> stops.</returns>
    public Task StopAsync()
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("Server is not running.");
        }

        StopServer();
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void DisconnectUser(Guid userId)
    {
        if (!_connectedUsers.TryRemove(userId, out TUser? user))
        {
            _logger?.LogError($"Cannot find user with id '{userId}'.");
            return;
        }

        _logger?.LogTrace($"User with id '{userId}' disconnected.");
        user.OnDisconnected();
        user.Dispose();
    }

    /// <inheritdoc/>
    public void DisconnectUser(LiteConnection connection)
    {
        DisconnectUser(connection.Id);
    }

    /// <inheritdoc/>
    public void SendTo(LiteConnection connection, byte[] packet)
    {
        if (connection is null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        if (packet is null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        connection.Send(packet);
    }

    /// <inheritdoc/>
    public void SendTo(IEnumerable<LiteConnection> connections, byte[] packet)
    {
        if (connections is null)
        {
            throw new ArgumentNullException(nameof(connections));
        }

        if (packet is null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        foreach (LiteConnection connection in connections)
        {
            SendTo(connection, packet);
        }
    }

    /// <inheritdoc/>
    public void SendToAll(byte[] packet) => SendTo(_connectedUsers.Values, packet);

    /// <summary>
    /// Dispose the server resources and disconnects all the connected users.
    /// </summary>
    public void Dispose()
    {
        if (IsRunning)
        {
            StopServer();
        }

        _socket.Dispose();
        _acceptor.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Executes the child business logic before starting the server.
    /// </summary>
    protected virtual void OnBeforeStart() { }

    /// <summary>
    /// Executes the child business logic after the server starts.
    /// </summary>
    protected virtual void OnAfterStart() { }

    /// <summary>
    /// Executes the child business logic before stoping the server.
    /// </summary>
    protected virtual void OnBeforeStop() { }

    /// <summary>
    /// Executes the child business logic after the server stops.
    /// </summary>
    protected virtual void OnAfterStop() { }

    /// <summary>
    /// Called when an error occurs on the server.
    /// </summary>
    /// <param name="connection">Connection where the error occured.</param>
    /// <param name="exception">Error exception.</param>
    protected virtual void OnError(TUser? connection, Exception exception)
    {
        if (connection is null)
        {
            _logger?.LogError(exception, $"An error has occured.");
        }
        else
        {
            _logger?.LogError(exception, $"An error has occured for user '{connection.Id}'.");
        }
    }

    private void OnClientAccepted(object? sender, SocketAsyncEventArgs e)
    {
        TUser user = _serviceProvider != null ? ActivatorUtilities.CreateInstance<TUser>(_serviceProvider) : Activator.CreateInstance<TUser>();

        if (!_connectedUsers.TryAdd(user.Id, user))
        {
            throw new LiteNetworkException($"Failed to add user with id: '{user.Id}'. An user with same id already exists.");
        }

        if (e.AcceptSocket is null)
        {
            throw new LiteNetworkException($"The accepted socket is null.");
        }

        user.Socket = e.AcceptSocket;
        user.Context = new LiteServerContext(this);
        user.InitializeSender(Options.PacketProcessor);
        _logger?.LogInformation($"New user connected from '{user.Socket.RemoteEndPoint}' with id '{user.Id}'.");
        user.OnConnected();
        _receiver.StartReceiving(user);
    }

    private void OnAcceptorError(object? sender, Exception e)
    {
        OnError(sender as TUser, e);
    }

    private void OnReceiverError(object? sender, Exception e)
    {
        OnError(sender as TUser, e);
    }

    private void OnDisconnected(object? _, LiteConnection e)
    {
        DisconnectUser(e.Id);
    }

    private void DisconnectAllUsers()
    {
        foreach (var connectedUser in _connectedUsers)
        {
            DisconnectUser(connectedUser.Key);
        }

        _connectedUsers.Clear();
    }

    private void StopServer()
    {
        OnBeforeStop();
        DisconnectAllUsers();
        IsRunning = false;
        OnAfterStop();
    }
}
