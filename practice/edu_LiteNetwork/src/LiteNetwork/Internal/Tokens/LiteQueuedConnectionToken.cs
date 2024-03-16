using LiteNetwork.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Internal.Tokens;

/// <summary>
/// Provides a mechanism used for the receive process.
/// </summary>
internal class LiteQueuedConnectionToken : ILiteConnectionToken
{
    private readonly Func<LiteConnection, byte[], Task> _handlerAction;
    private readonly BlockingCollection<byte[]> _receiveMessageQueue;
    private readonly CancellationToken _receiveCancellationToken;
    private readonly CancellationTokenSource _receiveCancellationTokenSource;

    public LiteConnection Connection { get; }

    public LiteDataToken DataToken { get; }

    /// <summary>
    /// Creates a new <see cref="LiteDefaultConnectionToken"/> instance with a <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="connection">Current connection.</param>
    /// <param name="handlerAction">Action to execute when a packet message is being processed.</param>
    public LiteQueuedConnectionToken(LiteConnection connection, Func<LiteConnection, byte[], Task> handlerAction)
    {
        Connection = connection;
        _handlerAction = handlerAction;
        DataToken = new LiteDataToken(Connection);
        _receiveMessageQueue = new BlockingCollection<byte[]>();
        _receiveCancellationTokenSource = new CancellationTokenSource();
        _receiveCancellationToken = _receiveCancellationTokenSource.Token;
        Task.Factory.StartNew(OnProcessMessageQueue,
            _receiveCancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public void Dispose()
    {
        _receiveCancellationTokenSource.Cancel();

        while (_receiveMessageQueue.Count > 0)
        {
            _receiveMessageQueue.Take();
        }
    }

    /// <summary>
    /// Processes the received message queue until cancellation is requested.
    /// </summary>
    private async Task OnProcessMessageQueue()
    {
        while (!_receiveCancellationToken.IsCancellationRequested)
        {
            try
            {
                byte[] message = _receiveMessageQueue.Take(_receiveCancellationToken);
                await _handlerAction(Connection, message).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // The operation has been cancelled: nothing to do
            }
        }
    }

    public void ProcessReceivedMessages(IEnumerable<byte[]> messages)
    {
        foreach (byte[] message in messages)
        {
            _receiveMessageQueue.Add(message);
        }
    }
}
