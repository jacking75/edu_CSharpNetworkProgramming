using laster40Net;
using System;
using System.Threading;

namespace laster40Net_EchoServer
{
    class Program
    {
        public const string IPString = "";
        public const int Port = 10000;
        public const int ReceiveBuffer = 4 * 1024;
        public const int MaxConnectionCount = 10000;
        public const int Backlog = 100;
        public static TcpService service = null;

        static void ConnectionCallback(long session, bool success, System.Net.EndPoint address, Object token)
        {
            if (success)
            {
                Console.WriteLine("[{0}]접속 완료 - id:{1}, remote:{2}", Thread.CurrentThread.GetHashCode(), session, address);
            }
            else
            {
                Console.WriteLine("[{0}]접속 실패 - id:{1}, remote:{2}", Thread.CurrentThread.GetHashCode(), session, address);
            }
        }

        static void CloseCallback(long session, CloseReason reason)
        {
            Console.WriteLine("[{0}]접속 종료 - remote:{1},id:{2}", Thread.CurrentThread.GetHashCode(), session, reason.ToString());
        }

        static void ReceiveCallback(long session, byte[] buffer, int offset, int length)
        {
            service.SendToSession(session, buffer, offset, length, true);
        }

        static void MesssageCallback(long session, byte[] buffer, int offset, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                if (buffer[offset + i] != (byte)i)
                    Console.WriteLine("이상하넹");
            }
            Console.WriteLine("[{0}]메세지 받음 - id:{1}, offset:{2}, length:{3}", Thread.CurrentThread.GetHashCode(), session, offset, length);
            service.SendToSession(session, buffer, offset, length, true);
        }

        static void Main(string[] args)
        {
            service = new TcpService("SimpleEchoServer.xml");
            service.ConnectionEvent += new SessionConnectionEvent(ConnectionCallback);
            service.CloseEvent += new SessionCloseEvent(CloseCallback);
            service.ReceiveEvent += new SessionReceiveEvent(ReceiveCallback);
            service.MessageEvent += new SessionMessageEvent(MesssageCallback);
            service.Run();

            Console.WriteLine("starting server!");

            int update = Environment.TickCount;
            while (true)
            {
                System.Threading.Thread.Sleep(50);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == '1')
                    {
                        service.Stop();
                    }
                    else if (key.KeyChar == '2')
                    {
                        service.Run();
                    }
                    else if (key.KeyChar == '3')
                    {
                    }
                    else if (key.KeyChar == '4')
                    {
                        service.StartConnect("127.0.0.1", Port, 1000, 0, null);
                    }
                    else if (key.KeyChar == 'q')
                    {
                        break;
                    }
                }

                if (Environment.TickCount - update > 5000)
                {
                    Console.WriteLine(service.ToString());
                    update = Environment.TickCount;
                }
            }

            service.Stop();
        }
    }
}
