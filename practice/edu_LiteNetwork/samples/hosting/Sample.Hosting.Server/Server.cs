using LiteNetwork.Server;
using System;

namespace LiteNetwork.Samples.Hosting.Server
{
    public class Server : LiteServer<ServerUser>
    {
        public Server(LiteServerOptions options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }

        protected override void OnBeforeStart()
        {
            Console.WriteLine("Starting server...");
        }

        protected override void OnAfterStart()
        {
            Console.WriteLine($"Server listening on port {Options.Port}.");
        }
    }
}
