using LiteNetwork.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiteNetwork.Internal.Tokens;

/// <summary>
/// Provides a data structure representing a lite connection token used for the receive process.
/// </summary>
internal class LiteDefaultConnectionToken : ILiteConnectionToken
{
    private readonly Func<LiteConnection, byte[], Task> _handlerAction;

    public LiteConnection Connection { get; }

    public LiteDataToken DataToken { get; }

    /// <summary>
    /// Creates a new <see cref="LiteDefaultConnectionToken"/> instance with a <see cref="LiteConnection"/>.
    /// </summary>
    /// <param name="connection">Current connection.</param>
    /// <param name="handlerAction">Asynchronous action to execute when a packet message is being processed.</param>
    public LiteDefaultConnectionToken(LiteConnection connection, Func<LiteConnection, byte[], Task> handlerAction)
    {
        Connection = connection;
        _handlerAction = handlerAction;
        DataToken = new LiteDataToken(Connection);
    }

    public void Dispose()
    {
        // nothing to do
    }

    public void ProcessReceivedMessages(IEnumerable<byte[]> messages)
    {
        Task.Run(async () =>
        {
            foreach (var messageBuffer in messages)
            {
                await _handlerAction(Connection, messageBuffer).ConfigureAwait(false);
            }
        });
    }
}
