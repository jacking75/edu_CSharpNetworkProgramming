using LiteNetwork.Server.Abstractions;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Server.Hosting;

/// <summary>
/// Define a basic <see cref="IHostedService"/> to use with <see cref="LiteServer{TUser}"/>.
/// </summary>
internal class LiteServerHostedService: IHostedService
{
    private readonly ILiteServer _server;

    /// <summary>
    /// Creates a new <see cref="LiteServerHostedService"/> with the given server to host.
    /// </summary>
    /// <param name="server">Server to host.</param>
    public LiteServerHostedService(ILiteServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server), $"Failed to inject server.");
    }

    public async Task StartAsync(CancellationToken cancellationToken) => await _server.StartAsync(cancellationToken);

    public async Task StopAsync(CancellationToken _) => await _server.StopAsync();
}
