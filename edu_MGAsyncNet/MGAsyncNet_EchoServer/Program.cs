using MGAsyncNet;
using System;

namespace MGAsyncNet_EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = " ";
            Packet p = new Packet();
            p.WriteString(s);

            try
            {
                var receiver = new EchoServer();

                int maxConnect = 1024;
                AsyncIOManager netserver = new AsyncIOManager(4, receiver, 1024, maxConnect, 512);

                AsyncSocket.InitUIDAllocator(1, (UInt64)maxConnect);

                Acceptor acceptor = new Acceptor(netserver, "192.168.0.79", 3210);
                acceptor.Start();

                while (true)
                {
                    // 이 예제는 연결된 클라이언트들에게 2초마다 뭔가를 날려줍니다요~
                    System.Threading.Thread.Sleep(2000);
                    receiver.Process();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
