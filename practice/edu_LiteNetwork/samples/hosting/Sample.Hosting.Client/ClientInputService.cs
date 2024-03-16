using Microsoft.Extensions.Hosting;

namespace LiteNetwork.Samples.Hosting.Server
{
    internal class ClientInputService : IHostedService
    {
        private readonly Client _client;

        public ClientInputService(Client client)
        {
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_client.IsConnected)
                {
                    Console.WriteLine("Waiting for connection...");
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                string? message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    _client.SendMessage(message);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    }
}
