using LiteNetwork.Server;
using System;

namespace Sample.CustomPacketReaderWriter.Server
{
    public class CustomServer : LiteServer<CustomServerUser>
    {
        public CustomServer(LiteServerOptions options)
            : base(options)
        {
        }

        protected override void OnBeforeStart()
        {
            Console.WriteLine("Starting Echo server.");
        }

        protected override void OnAfterStart()
        {
            Console.WriteLine($"Echo server listining on port: {Options.Port}");
        }
    }
}
