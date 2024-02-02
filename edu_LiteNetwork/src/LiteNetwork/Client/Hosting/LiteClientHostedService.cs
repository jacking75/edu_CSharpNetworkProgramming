using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Client.Hosting;

/// <summary>
/// Defines a basic <see cref="IHostedService"/> to use with <see cref="LiteClient"/>
/// </summary>
internal class LiteClientHostedService : IHostedService
{
    private readonly LiteClient _client;

    /// <summary>
    /// Creates a new <see cref="LiteClientHostedService"/> with the given server.
    /// </summary>
    /// <param name="client">Client to host.</param>
    public LiteClientHostedService(LiteClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "Failed to inject client.");
    }

    public async Task StartAsync(CancellationToken cancellationToken) => await _client.ConnectAsync();

    public async Task StopAsync(CancellationToken cancellationToken) => await _client.DisconnectAsync();
}
