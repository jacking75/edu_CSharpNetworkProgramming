using laster40Net;
using System;

namespace laster40Net_SimpleChatServer
{
    class Program
    {
        public const string IPString = "";
        public const int Port = 10000;
        public const int ReceiveBuffer = 4 * 1024;
        public const int MaxConnectionCount = 10000;
        public const int Backlog = 100;
        public static TcpService service = null;
        public static ChatServer server = null;

        static void ConnectionCallback(long session, bool success, System.Net.EndPoint address, Object token)
        {
            if (success)
            {
                Console.WriteLine("[{0}]접속 완료 - id:{1}, remote:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, session, address);
                server.HandleNewClient(session);
            }
            else
            {
                Console.WriteLine("[{0}]접속 실패 - id:{1}, remote:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, session, address);
            }
        }

        static void CloseCallback(long session, CloseReason reason)
        {
            server.HandleRemoveClient(session);
            Console.WriteLine("[{0}]접속 종료 - remote:{1},id:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, session, reason.ToString());
        }

        static void ReceiveCallback(long session, byte[] buffer, int offset, int length)
        {
        }

        static void MesssageCallback(long session, byte[] buffer, int offset, int length)
        {
            server.HandleMessage(session, buffer, offset, length);
        }


        static void Main(string[] args)
        {
            TcpServiceConfig config = new TcpServiceConfig();
            config.ReceviceBuffer = ReceiveBuffer;
            config.SendBuffer = ReceiveBuffer;
            config.SendCount = 10;
            config.MaxConnectionCount = MaxConnectionCount;
            config.UpdateSessionIntval = 50;
            config.SessionReceiveTimeout = 0;// 30 * 1000;

            config.MessageFactoryAssemblyName = "NetService";
            config.MessageFactoryTypeName = "NetService.Message.SimpleBinaryMessageFactory";

            service = new TcpService(config);

            service.ConnectionEvent += new SessionConnectionEvent(ConnectionCallback);
            service.CloseEvent += new SessionCloseEvent(CloseCallback);
            service.ReceiveEvent += new SessionReceiveEvent(ReceiveCallback);
            service.MessageEvent += new SessionMessageEvent(MesssageCallback);

            server = new ChatServer(service);

            service.Run();
            service.StartListener(IPString, Port, Backlog);

            Console.WriteLine("starting server!");

            int update = Environment.TickCount;
            while (true)
            {
                System.Threading.Thread.Sleep(50);
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.KeyChar == 'q')
                {
                    break;
                }
                if (Environment.TickCount - update > 5000)
                {
                    Console.WriteLine(service.ToString());
                    update = Environment.TickCount;
                }
            }

            service.Stop();
        }


    } // end Class
}
