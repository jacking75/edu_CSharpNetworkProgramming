using LiteNetwork.Protocol.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Internal;

/// <summary>
/// Provides a mechanism to send data.
/// </summary>
internal class LiteSender : IDisposable
{
    private readonly BlockingCollection<byte[]> _sendingCollection;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    private readonly LiteConnection _connection;
    private readonly ILitePacketProcessor _packetProcessor;
    private readonly SocketAsyncEventArgs _socketAsyncEvent;
    private bool _disposedValue;

    public event EventHandler<Exception>? Error;

    /// <summary>
    /// Gets a boolean value that indiciates if the sender process is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Creates and initializes a new <see cref="LiteSender"/> base instance.
    /// </summary>
    public LiteSender(LiteConnection connection, ILitePacketProcessor packetProcessor)
    {
        _sendingCollection = new BlockingCollection<byte[]>();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _connection = connection;
        _packetProcessor = packetProcessor;
        _socketAsyncEvent = new SocketAsyncEventArgs();
        _socketAsyncEvent.Completed += OnSendCompleted;
    }

    /// <summary>
    /// Starts the sender process.
    /// </summary>
    public void Start()
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Sender is already running.");
        }

        Task.Factory.StartNew(ProcessSendingQueue,
            _cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        IsRunning = true;
    }

    /// <summary>
    /// Stops the sender process.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("Sender is not running.");
        }

        _cancellationTokenSource.Cancel(false);
        IsRunning = false;
    }

    /// <summary>
    /// Sends a message to the current connection.
    /// </summary>
    /// <param name="messageData">Message data buffer to be sent.</param>
    public void Send(byte[] messageData) => _sendingCollection.Add(messageData);

    /// <summary>
    /// Dequeue the message collection and sends the messages to their recipients.
    /// </summary>
    private void ProcessSendingQueue()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                byte[] message = _sendingCollection.Take(_cancellationToken);
                message = _packetProcessor.AppendHeader(message);

                _socketAsyncEvent.SetBuffer(message, 0, message.Length);

                if (_connection.Socket != null && !_connection.Socket.SendAsync(_socketAsyncEvent))
                {
                    OnSendCompleted(this, _socketAsyncEvent);
                }
            }
            catch (OperationCanceledException)
            {
                // The operation has been cancelled: nothing to do
            }
            catch (Exception e)
            {
                Error?.Invoke(this, e);
            }
        }
    }

    /// <summary>
    /// Fired when a send operation has been completed.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Socket async event arguments.</param>
    protected void OnSendCompleted(object? sender, SocketAsyncEventArgs e)
    {
        _socketAsyncEvent.SetBuffer(null, 0, 0);
    }

    /// <summary>
    /// Disposes the sender resources.
    /// </summary>
    /// <param name="disposing">Disposing value.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (IsRunning)
                {
                    Stop();
                }
                _sendingCollection.Dispose();
                _cancellationTokenSource.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Dispose the <see cref="LiteSender"/> resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
