using LiteNetwork.Server;
using System;
using System.Threading.Tasks;

namespace LiteNetwork.Sample.Echo.Server;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== ECHO SERVER ===");
        
        var configuration = new LiteServerOptions()
        {
            Host = "127.0.0.1",
            Port = 4444,
            ReceiveStrategy = ReceiveStrategyType.Queued
        };
        using var server = new EchoServer(configuration);

        await server.StartAsync();
        Console.ReadKey();
    }
}
