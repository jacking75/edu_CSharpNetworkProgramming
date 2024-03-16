using System;
using System.Collections.Generic;
using System.Text;

using MGAsyncNet;

namespace MGAsyncNet_EchoServer
{
    class EchoServer : INetworkReceiver
    {
        public Dictionary<UInt64, AsyncSocketContext> theSessions = new Dictionary<UInt64, AsyncSocketContext>();

        // addressinfo => TcpClient.Client.RemoteEndPoint
        // 새로운 연결이 들어왔습니다.
        public void OnRegisterSocket(AsyncSocketContext sockdesc, string addressinfo)
        {
            //TODO Console.WriteLine은 모두 NLog로 바꾸기
            Console.WriteLine(sockdesc.ManagedID + " Connected");

            lock (this)
            {
                theSessions.Add(sockdesc.ManagedID, sockdesc);
            }
        }

        // 소켓연결이 해제되었습니다.
        public void OnReleaseSocket(AsyncSocketContext sockdesc, AsyncSocket socket)
        {
            Console.WriteLine(sockdesc.ManagedID + " Disconnected");

            lock (this)
            {
                theSessions.Remove(sockdesc.ManagedID);
            }

            sockdesc.NetSender.ReleaseAsyncSocketContext(sockdesc);
        }

        // connectSocket에 대한 결과
        //  bSuccess가 false이면 ex가 null이 아닌 개체로 전송됩니다~
        public void OnConnectingResult(int requestID, AsyncSocketContext sockdesc, bool bSuccess, Exception ex)
        { }

        public void OnReceiveData(AsyncSocketContext sockdesc, int length, byte[] data, int offset)
        {
            byte[] buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, length);

            if ('q' == buffer[0])
            {
                sockdesc.NetSender.DisconnectSocket(sockdesc);
            }
            else
            {
                sockdesc.NetSender.PostingSend(sockdesc, buffer.Length, buffer);
            }
        }

        public void Process()
        {
            // 2012-9-21 이 예제는 Process와 notifyRegisterSocket/notifyReleaseSocket이 동시에 실행되면 데드락을 일으킬수 있다.
            //  notify를 호출하는 asio측에선 자신을 lock하고 receiver의 lock을 대기한다.
            //  postingSend를 호출하는 receiver는 자신을 lock하고 asio의 lock을 대기한다.
            //  데드락을 없애는 요점은 asio계층의 쓰레드로 receiver가 해야하는 일을 직접 처리하지만 않으면 된다.. 큐잉하고 메인쓰레드가 디큐해서 실제 처리하도록~
            lock (this)
            {
                foreach (var node in theSessions)
                {
                    for (int i = 1; i <= 20; ++i)
                    {
                        string buffer = node.Value.ManagedID + "," + i + "\r\n";
                        byte[] bStrByte = System.Text.Encoding.Unicode.GetBytes(buffer);
                        //byte[] bStrByte = Encoding.UTF8.GetBytes(buffer);
                        node.Value.NetSender.PostingSend(node.Value, bStrByte.Length, bStrByte);
                    }
                }
            }
        }

    }

}
