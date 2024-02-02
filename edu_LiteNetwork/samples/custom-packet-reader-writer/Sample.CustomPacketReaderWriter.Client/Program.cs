using LiteNetwork.Client;
using Sample.CustomPacketReaderWriter.Client;
using Sample.CustomPacketReaderWriter.Protocol;
using System;

Console.WriteLine("=== ECHO CLIENT ===");

LiteClientOptions options = new()
{
    Host = "127.0.0.1",
    Port = 4444
};
CustomClient client = new(options);
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

    using var packetWriter = new CustomPacketWriter();

    packetWriter.WriteString(messageToSend);

    client.Send(packetWriter);
}

Console.WriteLine("Leaving program.");
Console.ReadKey();