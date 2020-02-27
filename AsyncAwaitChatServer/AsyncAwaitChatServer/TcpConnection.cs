using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ServerNet
{
    internal sealed class TcpConnection : IEquatable<TcpConnection>, IComparable<TcpConnection>, IAsyncDisposable
    {
        private readonly SemaphoreSlim sync;
        private readonly IPEndPoint endpoint;
        private readonly Socket socket;
        private readonly MemoryPool<byte> pool;
        private readonly System.Threading.Channels.ChannelWriter<IncomingMessage> channelWriter;
        private readonly NetworkStream stream;
        
        private readonly Memory<byte> writeLength = new byte[sizeof(int)];
        private readonly Memory<byte> readLength = new byte[sizeof(int)];
        private Task poolerTask;
        
        public TcpConnection(Socket socket,
            MemoryPool<byte> pool,
            System.Threading.Channels.ChannelWriter<IncomingMessage> channelWriter)
        {
            this.sync = new SemaphoreSlim(1);
            this.socket = socket;
            this.pool = pool;
            this.channelWriter = channelWriter;
            this.endpoint = (IPEndPoint) socket.RemoteEndPoint;
            this.stream = new NetworkStream(this.socket, ownsSocket: true);
        }

        /// <summary>
        /// Remote endpoint where this TCP connection can be accessed to.
        /// </summary>
        public IPEndPoint Endpoint => endpoint;

        public async ValueTask SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken)
        {
            await sync.WaitAsync(cancellationToken);
            try
            {
                // prefix-length encoding (Big Endian)
                BinaryPrimitives.WriteInt32BigEndian(writeLength.Span, (int)data.Length);
                await stream.WriteAsync(writeLength, cancellationToken);

                foreach (var buffer in data)
                {
                    await stream.WriteAsync(buffer, cancellationToken);
                }

                await stream.FlushAsync(cancellationToken);
            }
            finally
            {
                sync.Release();
            }
        }

        public Task Start()
        {
            Interlocked.CompareExchange(ref poolerTask, PoolMessages(default), null);
            return poolerTask;
        }

        /// <summary>
        /// Returns an async stream of messages send by the other side of this <see cref="TcpConnection"/>.
        /// </summary>
        private async Task PoolMessages(CancellationToken cancellationToken)
        {
            var message = await ReadNextAsync(cancellationToken);
            while (message.HasValue)
            {
                await channelWriter.WriteAsync(message.Value, cancellationToken);
                message = await ReadNextAsync(cancellationToken);
            }
        }
        
        private async Task<IncomingMessage?> ReadNextAsync(CancellationToken cancellationToken)
        {
            var read = await stream.ReadAsync(readLength, cancellationToken);
            if (read < sizeof(int))
            {
                return null; // connection closing
            }
            else
            {
                var messageLength = BinaryPrimitives.ReadInt32BigEndian(readLength.Span);
                var remaining = messageLength;
                var ownedMemory = this.pool.Rent(remaining);
                var buffer = ownedMemory.Memory.Length > remaining
                    ? ownedMemory.Memory.Slice(0, remaining)
                    : ownedMemory.Memory;
                
                while (remaining > 0)
                {
                    read = await stream.ReadAsync(buffer, cancellationToken);
                    if (read > 0)
                    {
                        buffer = buffer.Slice(read);
                        remaining -= read;  
                    }
                    // else if (remaining > 0)
                    // {
                    //     throw new IOException($"Connection {Endpoint} has been closed before the whole message payload was read.");
                    // }
                    else
                    {
                        break;
                    }
                }
                return new IncomingMessage(Endpoint, ownedMemory, messageLength);
            }
        }

        private int ReadMessageLength(ref ReadOnlySequence<byte> buffer)
        {
            var span = buffer.FirstSpan.Slice(0, 4);
            buffer = buffer.Slice(4);
            return BinaryPrimitives.ReadInt32BigEndian(span);
        }

        public ValueTask DisposeAsync()
        {
            poolerTask.Dispose();
            return stream.DisposeAsync();
        }

        #region equality and comparison

        public bool Equals(TcpConnection other)
        {
            if (other is null) return false;
            if (this.endpoint.Port != other.endpoint.Port) return false;
            if (this.endpoint.AddressFamily != other.endpoint.AddressFamily) return false;

            ReadOnlySpan<byte> a = this.endpoint.Address.GetAddressBytes();
            ReadOnlySpan<byte> b = other.endpoint.Address.GetAddressBytes();

            return a.SequenceEqual(b);
        }

        public int CompareTo(TcpConnection other)
        {
            if (other is null) return 1;
            
            var cmp = ((int)this.endpoint.AddressFamily).CompareTo((int)other.endpoint.AddressFamily);
            if (cmp == 0)
            {
                ReadOnlySpan<byte> a = this.endpoint.Address.GetAddressBytes();
                ReadOnlySpan<byte> b = other.endpoint.Address.GetAddressBytes();

                cmp = a.SequenceCompareTo(b);
                if (cmp == 0) return this.endpoint.Port.CompareTo(other.endpoint.Port);
            }

            return cmp;
        }
        
        #endregion
    }
}