using FastSocketLite.Server;
using FastSocketLite.SocketBase;
using System;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SocketServerManager.Init();
                SocketServerManager.Start();

                //10 초마다 모든 연결이 끊어 지도록 한다.
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(1000 * 10);
                        IHost host;
                        if (SocketServerManager.TryGetHost("quickStart", out host))
                        {
                            var arr = host.ListAllConnection();
                            foreach (var c in arr)
                            {
                                c.BeginDisconnect();
                            }
                        }
                    }
                });

                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[Exception: {ex.ToString()}");
            }
        }
    }
}
