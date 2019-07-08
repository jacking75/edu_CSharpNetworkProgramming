//출처 http://sabulinsprog.seesaa.net/category/7198317-1.html

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace BasicAsyncSocketServer
{
    class Program
    {
        


        static void Main(string[] args)
        {
            Console.WriteLine("Basic TCP Async Server");
        }


        // 송수신 전용 클래스
        private class SocketBuffer
        {
            public Socket Connection;
            public byte[] Buffer = new byte[1000];
        }

        private delegate void BeginAcceptDelegate(Socket listener);
        private static bool m_acceptLoop;

        static void AsyncServer()
        {
            // 로컬 IP와 포트번호
            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            int portNo = 50000;

            Console.WriteLine("---- Listener ----");
            Console.WriteLine("Local IP = {0}", localIP[0]);
            Console.WriteLine("Port No. = {0}", portNo);

            // listener 설정
            IPEndPoint ep = new IPEndPoint(localIP[0], portNo);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Tcp);
            listener.Bind(ep);

            listener.Listen(2);

            Console.WriteLine("1. Accept 시작");
            Console.WriteLine("2. 종료");
            Console.Write("-->");

            // 1 키를 누르면 시작
            // 2 키를 누르면 종료
            bool mainLoop = true;
            while (mainLoop)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                char com = keyInfo.KeyChar;

                switch (com)
                {
                    case '1':
                        Console.WriteLine("Accept 시작");
                        m_acceptLoop = true;
                        BeginAcceptDelegate accept = new BeginAcceptDelegate(BeginAccept);
                        accept.BeginInvoke(listener, null, null);
                        break;
                    case '2':
                        Console.WriteLine("종료");
                        m_acceptLoop = false;
                        mainLoop = false;
                        listener.Close();
                        break;
                    default:
                        break;
                }

            }
        }

        static void BeginAccept(Socket listener)
        {
            while (m_acceptLoop)
            {
                IAsyncResult ar = listener.BeginAccept(new AsyncCallback(AcceptCallback),
                                                       listener);
                // 비동기 accept 호출이 끝날 때까지 대기한다.
                ar.AsyncWaitHandle.WaitOne();
            }
        }

        static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;

            Socket connection = listener.EndAccept(ar);
            IPEndPoint remoteEP = (IPEndPoint)connection.RemoteEndPoint;
            Console.WriteLine("IP[{0}] Port[{1}] : Accepted!!\n",
                              remoteEP.Address,
                              remoteEP.Port);

            // 수신 대기
            SocketBuffer sb = new SocketBuffer();
            sb.Connection = connection;
            connection.BeginReceive(sb.Buffer,
                                    0,
                                    sb.Buffer.Length,
                                    SocketFlags.None,
                                    new AsyncCallback(ReceiveCallback), sb);

            Console.WriteLine("IP[{0}] Port[{1}] : 수신 대기 중...\n",
                              remoteEP.Address,
                              remoteEP.Port);
        }

        static void ReceiveCallback(IAsyncResult ar)
        {
            SocketBuffer sb = (SocketBuffer)ar.AsyncState;
            Socket connection = sb.Connection;
            IPEndPoint remoteEP = (IPEndPoint)connection.RemoteEndPoint;

            try
            {
                int receiveSize = connection.EndReceive(ar);
                string receiveString = Encoding.UTF8.GetString(sb.Buffer, 0, receiveSize);
                Console.WriteLine("IP[{0}] Port[{1}] : 수신 완료!",
                                  remoteEP.Address,
                                  remoteEP.Port);
                Console.WriteLine("수신 데이터: {0}\n", receiveString);

                // 송신
                string sendString =
                    string.Format("당신은 {0}입니다. \n수신 데이터는 {1}였습니다.",
                    remoteEP.Address, receiveString);
                Console.WriteLine("IP[{0}] Port[{1}] : 「{2}」를 송신 중\n",
                                  remoteEP.Address,
                                  remoteEP.Port,
                                  sendString);

                byte[] sendData = Encoding.UTF8.GetBytes(sendString);
                connection.BeginSend(sendData,
                                     0,
                                     sendData.Length,
                                     SocketFlags.None,
                                     new AsyncCallback(SendCallback),
                                     sb);
            }
            catch (SocketException)
            {
                // 수신 대기 중 클라이언트에서 접속을 끊은 경우 Exception이 발생한다.
                Console.WriteLine("IP[{0}] Port[{1}] : 리모트에서 접속이 끊어졌습니다.\n",
                                  remoteEP.Address,
                                  remoteEP.Port);
            }
        }

        static void SendCallback(IAsyncResult ar)
        {
            SocketBuffer sb = (SocketBuffer)ar.AsyncState;
            Socket connection = sb.Connection;
            IPEndPoint remoteEP = (IPEndPoint)connection.RemoteEndPoint;
            Console.WriteLine("IP[{0}] Port[{1}] : 송신 완료!\n",
                              remoteEP.Address,
                              remoteEP.Port);
        }
    }
}
