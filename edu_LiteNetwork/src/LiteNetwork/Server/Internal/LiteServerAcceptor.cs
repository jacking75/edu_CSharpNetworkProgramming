using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace LiteNetwork.Server.Internal;

/// <summary>
/// Accepts the clients into the server.
/// </summary>
internal class LiteServerAcceptor : IDisposable
{
    private readonly Socket _listeningSocket;
    private readonly SocketAsyncEventArgs _socketEvent;

    /// <summary>
    /// The event used when a client has been accepted.
    /// </summary>
    public event EventHandler<SocketAsyncEventArgs>? OnClientAccepted;

    /// <summary>
    /// The event used when an error has been occurred during the acceptation process.
    /// </summary>
    public event EventHandler<Exception>? OnError;

    /// <summary>
    /// Creates a new <see cref="LiteServerAcceptor"/> instance with the given <see cref="Socket"/>.
    /// </summary>
    /// <param name="serverSocket"><see cref="LiteServer{TUser}"/> listening socket.</param>
    public LiteServerAcceptor(Socket serverSocket)
    {
        _listeningSocket = serverSocket;
        _socketEvent = new SocketAsyncEventArgs();
        _socketEvent.Completed += OnSocketCompleted;
    }

    /// <summary>
    /// Starts accepting clients into the server.
    /// </summary>
    public void StartAccept()
    {
        if (_socketEvent.AcceptSocket is not null)
        {
            _socketEvent.AcceptSocket = null;
        }

        if (!_listeningSocket.AcceptAsync(_socketEvent))
        {
            ProcessAccept(_socketEvent);
        }
    }

    /// <summary>
    /// Process a new connected client.
    /// </summary>
    /// <param name="socketAsyncEvent">Socket async event arguments.</param>
    private void ProcessAccept(SocketAsyncEventArgs socketAsyncEvent)
    {
        if (socketAsyncEvent.SocketError == SocketError.Success)
        {
            try
            {
                OnClientAccepted?.Invoke(this, socketAsyncEvent);
            }
            catch (Exception exception)
            {
                OnError?.Invoke(this, exception);
            }

            StartAccept();
        }
    }

    /// <summary>
    /// Fired when a socket operation has completed.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Socket async event arguments.</param>
    [ExcludeFromCodeCoverage]
    private void OnSocketCompleted(object? sender, SocketAsyncEventArgs e)
    {
        try
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                ProcessAccept(e);
            }
            else
            {
                throw new InvalidOperationException($"Unknown '{e.LastOperation}' socket operation in accecptor.");
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Dispose the <see cref="LiteServerAcceptor"/> resources.
    /// </summary>
    public void Dispose()
    {
        _socketEvent.Dispose();
    }
}
