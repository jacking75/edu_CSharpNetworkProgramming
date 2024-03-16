using LiteNetwork.Client;
using Sample.CustomPacketReaderWriter.Protocol;
using System;
using System.Threading.Tasks;

namespace Sample.CustomPacketReaderWriter.Client
{
    public class CustomClient : LiteClient
    {
        public CustomClient(LiteClientOptions options, IServiceProvider serviceProvider = null)
            : base(options, serviceProvider)
        {
        }

        public override Task HandleMessageAsync(byte[] packetBuffer)
        {
            using var packetReader = new CustomPacketReader(packetBuffer);

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
}
