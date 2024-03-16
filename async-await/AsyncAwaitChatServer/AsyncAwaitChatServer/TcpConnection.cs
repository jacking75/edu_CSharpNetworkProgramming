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
    // ��ó: https://github.com/Horusiath/clusterpack/tree/master/src/ClusterPack/Transport 

    internal sealed class TcpConnection : IEquatable<TcpConnection>, IComparable<TcpConnection>, IAsyncDisposable
    {
        private readonly SemaphoreSlim Sync;
        private readonly IPEndPoint HostEndpoint;
        private readonly Socket Sock;
        private readonly MemoryPool<byte> Pool;
        private readonly System.Threading.Channels.ChannelWriter<IncomingMessage> ChannelWriter;
        private readonly NetworkStream Stream;
        
        private readonly Memory<byte> WriteLength = new byte[sizeof(int)];
        private readonly Memory<byte> ReadLength = new byte[sizeof(int)];
        private Task? PoolerTask;
        
        public TcpConnection(Socket socket,
            MemoryPool<byte> pool,
            System.Threading.Channels.ChannelWriter<IncomingMessage> channelWriter)
        {
            Sync = new SemaphoreSlim(1);
            Sock = socket;
            Pool = pool;
            ChannelWriter = channelWriter;
            HostEndpoint = (IPEndPoint) socket.RemoteEndPoint;

            //NetworkStream �� Socket�� ���� �������� ������ ����. NetworkStream�� Close�ϸ� Socket�� Close
            Stream = new NetworkStream(this.Sock, ownsSocket: true);
        }

        /// <summary>
        /// Remote endpoint where this TCP connection can be accessed to.
        /// </summary>
        public IPEndPoint Endpoint => Endpoint;

        // Sync ������ ����Ͽ� �񵿱� �Ϸᰡ ���� ���� �񵿱� ��û�� �� �� �ְ� ���ش�
        public async ValueTask SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken)
        {
            await Sync.WaitAsync(cancellationToken);
            try
            {
                //TODO ����� ���� ����� ������ ����. �ϳ��� ��ġ��

                // prefix-length encoding (Big Endian)
                BinaryPrimitives.WriteInt32BigEndian(WriteLength.Span, (int)data.Length);
                await Stream.WriteAsync(WriteLength, cancellationToken);

                foreach (var buffer in data)
                {
                    await Stream.WriteAsync(buffer, cancellationToken);
                }

                await Stream.FlushAsync(cancellationToken);
            }
            finally
            {
                Sync.Release();
            }
        }

        public Task Start()
        {
            Interlocked.CompareExchange(ref PoolerTask, PoolRemoteData(default), null);
            return PoolerTask;
        }

        /// <summary>
        /// Returns an async stream of messages send by the other side of this <see cref="TcpConnection"/>.
        /// </summary>
        private async Task PoolRemoteData(CancellationToken cancellationToken)
        {
            var message = await ReadNextAsync(cancellationToken);
            while (message.HasValue)
            {
                await ChannelWriter.WriteAsync(message.Value, cancellationToken);
                message = await ReadNextAsync(cancellationToken);
            }
        }
        
        private async Task<IncomingMessage?> ReadNextAsync(CancellationToken cancellationToken)
        {
            //TODO ReadAsync �ϳ��� ��ġ��
            var read = await Stream.ReadAsync(ReadLength, cancellationToken);
            if (read < sizeof(int))
            {
                return null; // connection closing
            }
            else
            {
                var messageLength = BinaryPrimitives.ReadInt32BigEndian(ReadLength.Span);
                var remaining = messageLength;
                var ownedMemory = this.Pool.Rent(remaining);
                var buffer = ownedMemory.Memory.Length > remaining
                    ? ownedMemory.Memory.Slice(0, remaining)
                    : ownedMemory.Memory;
                
                while (remaining > 0)
                {
                    read = await Stream.ReadAsync(buffer, cancellationToken);
                    if (read > 0)
                    {
                        buffer = buffer.Slice(read);
                        remaining -= read;  
                    }
                    else
                    {
                        break;
                    }
                }
                return new IncomingMessage(Endpoint, ownedMemory, messageLength);
            }
        }

        // ��� �б�
        private int ReadMessageLength(ref ReadOnlySequence<byte> buffer)
        {
            var span = buffer.FirstSpan.Slice(0, 4);
            buffer = buffer.Slice(4);
            return BinaryPrimitives.ReadInt32BigEndian(span);
        }

        public ValueTask DisposeAsync()
        {
            PoolerTask?.Dispose();
            return Stream.DisposeAsync();
        }


        //TcpConnection ��ü ��. Equals, CompareTo
        #region equality and comparison 

        public bool Equals(TcpConnection other)
        {
            if (other is null) return false;
            if (this.Endpoint.Port != other.Endpoint.Port) return false;
            if (this.Endpoint.AddressFamily != other.Endpoint.AddressFamily) return false;

            ReadOnlySpan<byte> a = this.Endpoint.Address.GetAddressBytes();
            ReadOnlySpan<byte> b = other.Endpoint.Address.GetAddressBytes();

            return a.SequenceEqual(b);
        }
                 
        public int CompareTo(TcpConnection other)
        {
            if (other is null) return 1;
            
            var cmp = ((int)this.Endpoint.AddressFamily).CompareTo((int)other.Endpoint.AddressFamily);
            if (cmp == 0)
            {
                ReadOnlySpan<byte> a = this.Endpoint.Address.GetAddressBytes();
                ReadOnlySpan<byte> b = other.Endpoint.Address.GetAddressBytes();

                cmp = a.SequenceCompareTo(b);
                if (cmp == 0) return this.Endpoint.Port.CompareTo(other.Endpoint.Port);
            }

            return cmp;
        }
        
        #endregion
    }
}