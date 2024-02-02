using LiteNetwork.Client;

namespace LiteNetwork.Samples.Hosting.Server
{
    internal class Client : LiteClient
    {
        public bool IsConnected { get; private set; }

        public Client(LiteClientOptions options, IServiceProvider serviceProvider = null!) 
            : base(options, serviceProvider)
        {
        }

        public override Task HandleMessageAsync(byte[] packetBuffer)
        {
            using var memoryStream = new MemoryStream(packetBuffer);
            using var binaryReader = new BinaryReader(memoryStream);
            string message = binaryReader.ReadString();

            Console.WriteLine($"Received from server: {message}");

            return Task.CompletedTask;
        }

        protected override void OnConnected()
        {
            Console.WriteLine("Client connected.");
            IsConnected = true;
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine("Disconnected");
            IsConnected = false;
            base.OnDisconnected();
        }

        public void SendMessage(string message)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(message);

            Send(stream);
        }
    }
}
