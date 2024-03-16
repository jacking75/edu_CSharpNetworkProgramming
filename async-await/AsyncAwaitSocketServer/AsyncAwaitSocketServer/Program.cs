using System;
using System.Net;

namespace AsyncAwaitSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AsyncAwait Echo Server");

            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 32452);
            var server = new AwaitServer(ipEndPoint);
            server.Run();
        }        
    }


    
}
