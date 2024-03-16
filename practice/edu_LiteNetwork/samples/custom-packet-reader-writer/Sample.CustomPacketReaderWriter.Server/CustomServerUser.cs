using LiteNetwork.Server;
using Sample.CustomPacketReaderWriter.Protocol;
using System;
using System.Threading.Tasks;

namespace Sample.CustomPacketReaderWriter.Server
{
    public class CustomServerUser : LiteServerUser
    {
        public override Task HandleMessageAsync(byte[] packetBuffer)
        {
            using var packetReader = new CustomPacketReader(packetBuffer);

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
            using var packetWriter = new CustomPacketWriter();

            packetWriter.WriteString(message);

            Send(packetWriter);
        }
    }
}
