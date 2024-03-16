using LiteNetwork.Internal;
using LiteNetwork.Protocol.Abstractions;
using System.Buffers;
using System.Net.Sockets;

namespace LiteNetwork.Server.Internal;

/// <summary>
/// Overrides the basic <see cref="LiteReceiver"/> for the server needs.
/// </summary>
internal class LiteServerReceiver : LiteReceiver
{
    private readonly ObjectPool<SocketAsyncEventArgs> _readPool = new(() => new SocketAsyncEventArgs());
    private readonly int _clientBufferSize;

    /// <summary>
    /// Creates a new <see cref="LiteServerReceiver"/> instance with the given <see cref="ILitePacketProcessor"/>
    /// and a client buffer size.
    /// </summary>
    /// <param name="packetProcessor">Current packet processor used in the server.</param>
    /// <param name="receiveStrategy">The receive strategy type for every received message.</param>
    /// <param name="clientBufferSize">Client buffer size defined in server configuration.</param>
    public LiteServerReceiver(ILitePacketProcessor packetProcessor, ReceiveStrategyType receiveStrategy, int clientBufferSize) 
        : base(packetProcessor, receiveStrategy)
    {
        _clientBufferSize = clientBufferSize;
    }

    protected override void ClearSocketEvent(SocketAsyncEventArgs socketAsyncEvent)
    {
        if (socketAsyncEvent.Buffer != null)
        {
            ArrayPool<byte>.Shared.Return(socketAsyncEvent.Buffer, true);
        }

        socketAsyncEvent.SetBuffer(null, 0, 0);
        socketAsyncEvent.UserToken = null;
        socketAsyncEvent.Completed -= OnCompleted;

        _readPool.Return(socketAsyncEvent);
    }

    protected override SocketAsyncEventArgs GetSocketEvent()
    {
        SocketAsyncEventArgs socketAsyncEvent = _readPool.Get();

        socketAsyncEvent.SetBuffer(ArrayPool<byte>.Shared.Rent(_clientBufferSize), 0, _clientBufferSize);
        socketAsyncEvent.Completed += OnCompleted;

        return socketAsyncEvent;
    }
}
