using LiteNetwork.Internal;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork;

/// <summary>
/// Provides an abstraction that represents a living connection.
/// </summary>
public abstract class LiteConnection : IDisposable
{
    private bool _disposed;
    private LiteSender? _sender = null;

    /// <summary>
    /// Gets the connection unique identifier.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the connection socket.
    /// </summary>
    public Socket? Socket { get; internal set; } = null;

    /// <summary>
    /// Default constructor for ever connection.
    /// </summary>
    protected LiteConnection()
    {
    }

    /// <summary>
    /// Handle an incoming packet message asynchronously.
    /// </summary>
    /// <param name="packetBuffer">Incoming packet buffer.</param>
    /// <returns>A <see cref="Task"/> that completes when finished the handle message operation.</returns>
    public abstract Task HandleMessageAsync(byte[] packetBuffer);

    /// <summary>
    /// Sends a raw <see cref="byte" /> array buffer to the remote end point.
    /// </summary>
    /// <param name="packetBuffer">Raw packet buffer as a byte array.</param>
    public virtual void Send(byte[] packetBuffer)
    {
        _sender?.Send(packetBuffer);
    }

    /// <summary>
    /// Sends a buffer contained into a <see cref="Stream"/> to the remote end point.
    /// </summary>
    /// <param name="packetStream">Packet stream.</param>
    public virtual void Send(Stream packetStream)
    {
        long oldPosition = packetStream.Position;
        byte[] packetBuffer = new byte[packetStream.Length];

        packetStream.Seek(0, SeekOrigin.Begin);
        packetStream.Read(packetBuffer, 0, packetBuffer.Length);
        packetStream.Seek(oldPosition, SeekOrigin.Begin);
        Send(packetBuffer);
    }

    /// <summary>
    /// Triggered when an error occured related with the current <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="exception"></param>
    protected virtual void OnError(object? sender, Exception exception)
    {
    }

    /// <summary>
    /// Initialize the <see cref="LiteConnection"/> sender.
    /// </summary>
    /// <param name="packetProcessor">Packet processor.</param>
    internal void InitializeSender(ILitePacketProcessor packetProcessor)
    {
        if (_sender is null)
        {
            _sender = new LiteSender(this, packetProcessor);
            _sender.Error += OnError;
        }

        _sender.Start();
    }

    /// <summary>
    /// Stops the sender.
    /// </summary>
    internal void StopSender()
    {
        _sender?.Stop();
    }

    /// <summary>
    /// Dispose a <see cref="LiteConnection"/> managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_sender != null)
                {
                    _sender.Error -= OnError;
                    _sender.Dispose();
                }

                Socket?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
