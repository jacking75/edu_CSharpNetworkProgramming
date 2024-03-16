using LiteNetwork.Client.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Client;

/// <summary>
/// Provides a basic TCP client implementation.
/// </summary>
public class LiteClient : LiteConnection
{
    /// <summary>
    /// The event used when the client has been connected.
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    /// The event used when the client has been disconnected.
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// The event used when the client has encountered an error.
    /// </summary>
    public event EventHandler<Exception>? Error;

    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<LiteClient>? _logger;
    private readonly LiteClientConnector _connector;
    private readonly LiteClientReceiver _receiver;

    /// <inheritdoc />
    public LiteClientOptions Options { get; }

    /// <summary>
    /// Creates a new <see cref="LiteClient"/> instance with the given <see cref="LiteClientOptions"/>.
    /// </summary>
    /// <param name="options">A client configuration options.</param>
    /// <param name="serviceProvider">Service provider to use.</param>
    public LiteClient(LiteClientOptions options, IServiceProvider? serviceProvider = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Options = options;

        _serviceProvider = serviceProvider;
        _connector = new LiteClientConnector(Socket, Options.Host, Options.Port);
        _receiver = new LiteClientReceiver(options.PacketProcessor, options.ReceiveStrategy, options.BufferSize);
        _receiver.Error += (s, e) => OnError(e);

        if (_serviceProvider is not null)
        {
            _logger = _serviceProvider.GetService<ILogger<LiteClient>>();
        }
    }

    /// <inheritdoc />
    public override Task HandleMessageAsync(byte[] packetBuffer) => Task.CompletedTask;

    /// <inheritdoc />
    public async Task ConnectAsync()
    {
        _logger?.LogTrace($"Connecting to {Options.Host}:{Options.Port}.");
        bool isConnected = await _connector.ConnectAsync();

        if (isConnected)
        {
            InitializeSender(Options.PacketProcessor);
            _receiver.StartReceiving(this);
            OnConnected();
            _logger?.LogTrace($"Connected to {Options.Host}:{Options.Port}.");
        }
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        _logger?.LogTrace($"Disconnecting from {Options.Host}:{Options.Port}.");
        bool isDisconnected = await _connector.DisconnectAsync();

        if (isDisconnected)
        {
            StopSender();
            OnDisconnected();
            _logger?.LogTrace($"Disconnected from {Options.Host}:{Options.Port}.");
        }
    }

    /// <summary>
    /// Fired when the client has been connected.
    /// </summary>
    protected virtual void OnConnected()
    {
        Connected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fired when the client has been disconnected.
    /// </summary>
    protected virtual void OnDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fired when the client has encountered an error.
    /// </summary>
    /// <param name="exception">Exception with the error.</param>
    protected virtual void OnError(Exception exception)
    {
        Error?.Invoke(this, exception);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connector.Dispose();

            _receiver.Error -= OnError;
            _receiver.Dispose();
        }

        base.Dispose(disposing);
    }
}
