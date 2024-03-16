using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwaitChatServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var token = System.Threading.CancellationToken.None;

            using var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();

            //NullLogger.Instance
            await using var transport = new ServerNet.TcpTransport(loggerFactory.CreateLogger("TCP local"));

            var localEndpoint = "127.0.0.1:32452";
            var messages = transport.BindAsync(IPEndPoint.Parse(localEndpoint), token);

            var reader = Task.Run(async () =>
            {
                await foreach (var incoming in messages.WithCancellation(token))
                {
                    using var msg = incoming;
                    var text = Encoding.UTF8.GetString(msg.Payload.FirstSpan);
                    Console.WriteLine($"Received message from '{msg.Endpoint}': {text}");
                }
            });

            Console.WriteLine($"Local host started at {transport.LocalEndpoint} ...");

            //var endpoints = new List<IPEndPoint>();
            //foreach (var hostWithPort in options.SeedNodes)
            //{
            //    var endpoint = IPEndPoint.Parse(hostWithPort);
            //    endpoints.Add(endpoint);
            //}

            Console.WriteLine("Ready to receive messages...");
            var userText = "";
            do
            {
                Console.Write("> ");
                userText = Console.ReadLine();
                //var bytes = new System.Buffers.ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(userText));

                //foreach (var endpoint in endpoints)
                //{
                //    await transport.SendAsync(endpoint, bytes, token);
                //    Console.WriteLine($"Sent '{userText}' to {endpoint}...");
                //}

            } while (userText != "quit");
        }
    }
    
}
