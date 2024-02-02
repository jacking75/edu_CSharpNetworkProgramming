using LiteNetwork.Server;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LiteNetwork.Sample.Echo.Server;

public class EchoUser : LiteServerUser
{
    public override Task HandleMessageAsync(byte[] packetBuffer)
    {
        using var incomingPacketStream = new MemoryStream(packetBuffer);
        using var packetReader = new BinaryReader(incomingPacketStream);

        string receivedMessage = packetReader.ReadString();

        Console.WriteLine($"Received from '{Id}': {receivedMessage}");
        SendMessage($"Received: '{receivedMessage}'.");

        return Task.CompletedTask;
    }

    protected override void OnConnected()
    {
        Console.WriteLine($"New client connected with id: {Id}");
        SendMessage($"Hello {Id}!");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Client '{Id}' disconnected.");
    }

    private void SendMessage(string message)
    {
        using var outgoingPacketStream = new MemoryStream();
        using var packetWriter = new BinaryWriter(outgoingPacketStream);

        packetWriter.Write(message);

        Send(packetWriter.BaseStream);
    }
}
