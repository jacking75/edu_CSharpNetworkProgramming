using LiteNetwork;
using LiteNetwork.Server;
using Sample.CustomPacketReaderWriter.Server;
using System;

Console.WriteLine("=== CUSTOM SERVER ===");

var configuration = new LiteServerOptions()
{
    Host = "127.0.0.1",
    Port = 4444,
    ReceiveStrategy = ReceiveStrategyType.Queued
};
using var server = new CustomServer(configuration);

await server.StartAsync();
Console.ReadKey();