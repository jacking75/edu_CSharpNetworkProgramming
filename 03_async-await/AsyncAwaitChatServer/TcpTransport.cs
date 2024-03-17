using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerNet
{
    // 출처: https://github.com/Horusiath/clusterpack/tree/master/src/ClusterPack/Transport 

    //MemoryPool<byte> memoryPool;  https://docs.microsoft.com/ko-kr/dotnet/api/system.buffers.memorypool-1?view=netcore-3.1

    public sealed class TcpTransport : ITransport, IAsyncDisposable
    {
        private readonly SemaphoreSlim Sync;
        private readonly ILogger Logger;
        private readonly TcpTransportOptions Options;
        private readonly Channel<IncomingMessage> IncommingMessageChan;
        private readonly ConcurrentDictionary<IPEndPoint, TcpConnection> Connections; //TODO key 바꾸기
        
        private Socket? ServerSock;
        private EndPoint? ServerEndpoint;
        private Task? AcceptorLoopTask;
        private int isDisposed = 0;

        public TcpTransport(ILogger logger, TcpTransportOptions? options = null
            )
        {
            options ??= new TcpTransportOptions();
            this.Sync = new SemaphoreSlim(1);
            this.Logger = logger;
            this.Options = options;
            this.IncommingMessageChan = Channel.CreateBounded<IncomingMessage>(options.IncomingMessageBufferSize);
            this.Connections = new ConcurrentDictionary<IPEndPoint, TcpConnection>();
        }

        /// <summary>
        /// Returns true if current TCP transport has been disposed.
        /// </summary>
        public bool IsDisposed => isDisposed != 0;
        
        /// <summary>
        /// Returns an endpoint, at which current TCP transport is listening, ready to accept new connections.
        /// This require to call <see cref="BindAsync"/> first, otherwise listener will not be started and
        /// a null value will be returned. 
        /// </summary>
        public EndPoint? LocalEndpoint => ServerEndpoint;

        /// <inheritdoc cref="ITransport"/>
        public async ValueTask SendAsync(IPEndPoint target, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
        {
            if (!Connections.TryGetValue(target, out var connection))
            {
                //TODO 접속하면 AddConnection 호출하기
                connection = await AddConnection(target, cancellationToken);
            }

            await connection.SendAsync(payload, cancellationToken);
            Logger.LogDebug("sent payload of size {0}B to {1}", payload.Length, target);
        }

        /// <inheritdoc cref="ITransport"/>
        public IAsyncEnumerable<IncomingMessage> BindAsync(EndPoint endpoint, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var previous = Interlocked.CompareExchange(ref ServerEndpoint, endpoint, null);
            if (previous is null)
            {
                this.ServerSock = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.ServerSock.Bind(endpoint);
                this.ServerSock.Listen(Options.Backlog);

                AcceptorLoopTask = Task.Factory.StartNew(AcceptConnections, cancellationToken);

                ServerEndpoint = this.ServerSock.LocalEndPoint;
                Logger.LogInformation("listening on '{0}'", ServerEndpoint);

                return MessageStream(cancellationToken);
            }
            else
            {
                throw new ArgumentException($"Cannot bind {nameof(TcpTransport)} to endpoint '{endpoint}', because it's already listening at {ServerEndpoint}", nameof(endpoint));
            }
        }

        private async IAsyncEnumerable<IncomingMessage> MessageStream(CancellationToken cancellationToken)
        {
            var reader = IncommingMessageChan.Reader;
            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.TryRead(out var message))
                {
                    if (cancellationToken.IsCancellationRequested)
                        yield break;

                    yield return message;
                }
            }
        }

        /// <summary>
        /// Asynchronously disconnects maintained TCP connection with a given <paramref name="endpoint"/>.
        /// Returns false if no active connection to a corresponding <paramref name="endpoint"/> was found. 
        /// </summary>
        public async ValueTask<bool> DisconnectAsync(IPEndPoint endpoint)
        {
            if (Connections.TryRemove(endpoint, out var connection))
            {
                await connection.DisposeAsync();
                return true;
            }

            return false;
        }

        private async Task<TcpConnection> AddConnection(IPEndPoint target, CancellationToken cancellationToken)
        {
            TcpConnection connection;
            await Sync.WaitAsync(cancellationToken);
            try
            {
                // we need to check again after lock was acquired
                if (!Connections.TryGetValue(target, out connection))
                {
                    var sock = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await sock.ConnectAsync(target);
                    connection = new TcpConnection(sock, MemoryPool<byte>.Shared, IncommingMessageChan.Writer);
                    Connections.TryAdd(target, connection);
                }
            }
            finally
            {
                Sync.Release();
            }

            _ = connection.Start();
            
            Logger.LogInformation("connected to '{0}'", target);
            
            return connection;
        }

        private async Task AcceptConnections()
        {
            while (!IsDisposed)
            {
                var incoming = await this.ServerSock.AcceptAsync().ConfigureAwait(false);
                var connection = new TcpConnection(incoming, MemoryPool<byte>.Shared, IncommingMessageChan.Writer);
                while (!this.Connections.TryAdd(connection.Endpoint, connection))
                {
                    if (this.Connections.TryRemove(connection.Endpoint, out var existing))
                    {
                        await existing.DisposeAsync();
                    }
                }

                _ = Task.Factory.StartNew(connection.Start).ConfigureAwait(false);
                Logger.LogInformation("Received incoming connection from '{0}'", connection.Endpoint);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
            {
                await Task.WhenAll(Connections.Values.Select(DisconnectAsync));
                
                AcceptorLoopTask?.Dispose();
                IncommingMessageChan.Writer.Complete();
                ServerSock?.Dispose();
            }
        }

        private async Task DisconnectAsync(TcpConnection connection)
        {
            try
            {
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "an exception happened while trying to closea TCP connection to '{0}'", connection.Endpoint);
            }
        }
    }
}