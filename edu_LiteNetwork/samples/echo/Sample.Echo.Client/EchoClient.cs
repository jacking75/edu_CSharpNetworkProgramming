﻿using LiteNetwork.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sample.Echo.Client;

public class EchoClient : LiteClient
{
    public EchoClient(LiteClientOptions options, IServiceProvider serviceProvider = null) 
        : base(options, serviceProvider)
    {
    }

    public override Task HandleMessageAsync(byte[] packetBuffer)
    {
        using var incomingPacketStream = new MemoryStream(packetBuffer);
        using var packetReader = new BinaryReader(incomingPacketStream);

        string message = packetReader.ReadString();

        Console.WriteLine($"Received from server: {message}");

        return Task.CompletedTask;
    }

    protected override void OnConnected()
    {
        Console.WriteLine("Client connected.");
        base.OnConnected();
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine("Disconnected");
        base.OnDisconnected();
    }
}
