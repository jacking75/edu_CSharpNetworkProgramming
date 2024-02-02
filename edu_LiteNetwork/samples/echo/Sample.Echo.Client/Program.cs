using LiteNetwork.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sample.Echo.Client;

internal class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== ECHO CLIENT ===");

        LiteClientOptions options = new()
        {
            Host = "127.0.0.1",
            Port = 4444
        };
        EchoClient client = new(options);
        Console.WriteLine("Press any key to connect to server.");
        Console.ReadKey();

        await client.ConnectAsync();

        while (client.Socket.Connected)
        {
            string messageToSend = Console.ReadLine();

            if (messageToSend == "quit")
            {
                await client.DisconnectAsync();
                break;
            }

            using var packetStream = new MemoryStream();
            using var packet = new BinaryWriter(packetStream);

            packet.Write(messageToSend);

            client.Send(packet.BaseStream);
        }

        Console.WriteLine("Leaving program.");
        Console.ReadKey();
    }
}
