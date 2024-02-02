using LiteNetwork.Server;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LiteNetwork.Samples.Hosting.Server
{
    public class ServerUser : LiteServerUser
    {
        public override Task HandleMessageAsync(byte[] packetBuffer)
        {
            using var memoryStream = new MemoryStream(packetBuffer);
            using var binaryReader = new BinaryReader(memoryStream);
            string receivedMessage = binaryReader.ReadString();

            Console.WriteLine($"Received from '{Id}': {receivedMessage}");

            return Task.CompletedTask;
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"New client connected with id: {Id}");

            using Stream welcomePacketStream = BuildWelcomePacket();

            Send(welcomePacketStream);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Client '{Id}' disconnected.");
        }

        private Stream BuildWelcomePacket()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write($"Hello {Id}!");

            return stream;
        }
    }
}
