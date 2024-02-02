using LiteNetwork.Client.Hosting;
using LiteNetwork.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiteNetwork.Samples.Hosting.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "LiteNetwork Hosting Sample (Client)";

            var host = new HostBuilder()
                .ConfigureLiteNetwork((context, builder) =>
                {
                    builder.AddLiteClient<Client>(options =>
                    {
                        options.Host = "127.0.0.1";
                        options.Port = 4444;
                    });
                })
                .UseConsoleLifetime()
                .Build();

            // Run the host build services in background.
            await Task.Factory.StartNew(async () => await host.RunAsync()).ConfigureAwait(false);

            // Process user input.
            bool isRunning = true;
            var client  = host.Services.GetRequiredService<Client>();
            while (isRunning)
            {
                if (!client.IsConnected)
                {
                    Console.WriteLine("Waiting for connection...");
                    await Task.Delay(1000);
                    continue;
                }

                string? message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    client.SendMessage(message);
                }
                else
                {
                    isRunning = false;
                }
            }
        }
    }
}
