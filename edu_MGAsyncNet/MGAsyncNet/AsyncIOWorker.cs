using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MGAsyncNet
{
    public class ConnectFrame
    {
        public int ReqID;
        public AsyncSocket Socket;
    };


    // TODO INetworkSender를 상속 받지 않도록 하자
    // TODO 대부분의 메소드에서 lock을 걸고 있는데 이것을 줄여보자
    internal class AsyncIOWorker : INetworkSender
    {
        public int Index;
        private AsyncIOManager IOManager;
        private INetworkReceiver NetReceiver;
        private Dictionary<UInt64, AsyncSocket> Sockets = new Dictionary<UInt64, AsyncSocket>();

        public AsyncIOWorker(INetworkReceiver receiver, AsyncIOManager asio)
        {
            NetReceiver = receiver;
            IOManager = asio;
        }

        public int PostingSend(AsyncSocketContext sockdesc, int length, byte[] data)
        {
            //TODO 최대한 lock을 안 잡도록 해보자
            // postingSend, ReleaseSocket(다른 워커쓰레드에서 ResultRead, ResultWrite에서 을 쓰레드락걸지 않으면,
            //  aSocket.sendlist를 위해서라도 락을 걸어야한다.
            lock (this)
            {
                // bugfix 찾지못한경우 널이 반환되는게 아니라 예외가 던져진다.
                AsyncSocket aSocket;
                try
                {
                    aSocket = Sockets[sockdesc.ManagedID];
                }
                catch (Exception /*ex*/)
                {
                    return (int)MyErrorCode.NoSuchSocket;
                }

                if (aSocket.Description.UniqueId != sockdesc.UniqueId)
                {
                    return (int)MyErrorCode.NoSuchSocket;
                }

                // asiomanager 생성시 io 크기보다 큰 센드 요청은 할수 없다.
                if (IOManager.IOMAXSIZE < length || 0 >= length)
                {
                    return (int)MyErrorCode.SizeError;
                }


                // sendlist쓰지말고 곧바로 보내버리자!!!
                SocketAsyncEventArgs writeEventArgs = IOManager.RetreiveEventArgs();
                if (null == writeEventArgs)
                {
                    // temp code critical outofmemory
                    return (int)MyErrorCode.OutOfMemory;
                }

                aSocket.EnterIO();

                try
                {
                    // send의 경우 setbuffer 자체가 보낼 버퍼를 지정하는것이다... setbuffer를 다시한번해서 하는게 안전할까?
                    writeEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    writeEventArgs.SetBuffer(writeEventArgs.Buffer, writeEventArgs.Offset, length);
                    Array.Copy(data, 0, writeEventArgs.Buffer, writeEventArgs.Offset, length);
                    writeEventArgs.UserToken = aSocket;

                    bool willRaiseEvent = aSocket.TCPClient.Client.SendAsync(writeEventArgs);

                    if (!willRaiseEvent)
                    {
                        ProcessSend(writeEventArgs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " - postingSend " + ex.Message);

                    if (null != writeEventArgs)
                    {
                        writeEventArgs.Completed -= IO_Completed;
                        IOManager.ReleaseSAEA(writeEventArgs);
                    }

                    ReleaseSocket(aSocket);
                }
            }
            return 0;
        }

        public int DisconnectSocket(AsyncSocketContext sockdesc)
        {
            lock (this)
            {
                // 단순히 Close하면되는것인가?
                // bugfix 찾지못한경우 널이 반환되는게 아니라 예외가 던져진다.
                // theSockets만 쓰레드컨트롤하면된다.
                AsyncSocket aSocket;
                try
                {
                    aSocket = Sockets[sockdesc.ManagedID];
                }
                catch (Exception /*ex*/)
                {
                    return (int)MyErrorCode.NoSuchSocket;
                }

                if (aSocket.Description.UniqueId != sockdesc.UniqueId)
                {
                    return (int)MyErrorCode.NoSuchSocket;
                }

                aSocket.TCPClient.GetStream().Close();
                aSocket.TCPClient.Close();
            }
            return 0;
        }

        public int ConnectSocket(int reqID, AsyncSocket socket, string ipaddress, int port)
        {
            lock (this)
            {
                if (true == Sockets.ContainsKey(socket.Description.ManagedID))
                {
                    return (int)MyErrorCode.AlreadyPostConnect;
                }

                // 동일한 socket에 대해 connect를 또 요청할수 있으니 미리 컨테이너에 추가하고 하자.
                Sockets.Add(socket.Description.ManagedID, socket);

                ConnectFrame connectFrame = new ConnectFrame();
                connectFrame.ReqID = reqID;
                connectFrame.Socket = socket;
                connectFrame.Socket.Description.NetSender = this;

                socket.TCPClient = new TcpClient(AddressFamily.InterNetwork);
                socket.TCPClient.BeginConnect(ipaddress, port, new AsyncCallback(ResultConnect), connectFrame);
            }

            return 0;
        }

        public int ReleaseAsyncSocketContext(AsyncSocketContext sockdesc)
        {
            AsyncSocket.ReleaseUID(sockdesc.ManagedID);
            return 0;
        }

        // c#에선 TcpListener.AcceptTcpClient가 반환하는 TcpClient객체레퍼런스를 넘겨줘야한다.
        //  즉 c++에선 int 값을 넘겨줬지만, c#에선 TcpClient 이 객체를 기본 베이스로 사용해야 한다.
        public int RegisterSocket(TcpClient acceptedclient)
        {            
            lock (this)
            {

                var aSocket = new AsyncSocket();
                aSocket.SetTcpClient(acceptedclient);
                aSocket.Description.NetSender = this;

                // 관리리스트에 등록 - need lock
                Sockets.Add(aSocket.Description.ManagedID, aSocket);

                //TODO 구현이 좀 이상한듯
                // 리시버에게 객체파괴가 어떻게 일어날지 모르니, desc는 값을 복사해서 사용하자.                
                AsyncSocketContext desc = new AsyncSocketContext();
                desc.UniqueId = aSocket.Description.UniqueId;
                desc.ManagedID = aSocket.Description.ManagedID;
                desc.NetSender = aSocket.Description.NetSender;
                NetReceiver.OnRegisterSocket(desc, aSocket.RemoteAddress);

                aSocket.EnterIO();

                // key saea얻어오기 실패하면 메모리부족이기 때문에, 프로세스를 죽여야한다.
                SocketAsyncEventArgs readEventArgs = IOManager.RetreiveEventArgs();// new SocketAsyncEventArgs();//= saeaPool.pop(); // saeaPool from the ASIOManager

                try
                {
                    readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    readEventArgs.UserToken = aSocket;
                    bool willRaiseEvent = aSocket.TCPClient.Client.ReceiveAsync(readEventArgs);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(readEventArgs);
                        // 위에서 releaseSocket이 되면, 아래 예외처리는 수행되지 않는다.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " - registerSocket " + ex.Message);
                    // releasesocket을 수정하지 말고 각 문맥에서 releaseSAEA를 호출하자
                    if (null != readEventArgs)
                    {
                        readEventArgs.Completed -= IO_Completed;
                        IOManager.ReleaseSAEA(readEventArgs);
                    }
                    ReleaseSocket(aSocket);
                }
            }
            
            return 0;
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncSocket token = (AsyncSocket)e.UserToken;
            try
            {                
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    token.HandleReceived(e.BytesTransferred, e.Buffer, e.Offset, NetReceiver);

                    bool willRaiseEvent = token.TCPClient.Client.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        // 이 소켓이 대해 계속 즉시 완료가 일어나면, 이 워커쓰레드는 이 소켓처리만 계속하게 될것이다. 하지만 그런경우는 드물다..
                        ProcessReceive(e);
                    }
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString() + " - processreceive  " + e.BytesTransferred + " " + e.SocketError);
                    
                    // e를 재사용하려면 e.Completed -= IO_Completed; 하면된다. 현재는 버퍼만 풀링하고 SocketAsyncEventArgs는 풀링하지 않는다.
                    e.Completed -= IO_Completed;
                    IOManager.ReleaseSAEA(e);
                    ReleaseSocket(token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - processreceive " + ex.Message);
                // 응용계층에서 프로토콜 파싱하는 과정에서 발생한 예외도 여기서 캐치되어 세션종료만을 해서 프로세스가 종료되지 않는다.
                e.Completed -= IO_Completed;
                IOManager.ReleaseSAEA(e);
                ReleaseSocket(token);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            // TODO 다 못보내져서 남은것을 재전송해야하는 경우는??? 
            AsyncSocket token = (AsyncSocket)e.UserToken;
            e.Completed -= IO_Completed;
            IOManager.ReleaseSAEA(e);
            ReleaseSocket(token);
            
        }


        private void ResultConnect(IAsyncResult ar)
        {
            ConnectFrame connectFrame = (ar.AsyncState as ConnectFrame);
            AsyncSocketContext desc = new AsyncSocketContext();
            desc.UniqueId = connectFrame.Socket.Description.UniqueId;
            desc.ManagedID = connectFrame.Socket.Description.ManagedID;
            desc.NetSender = connectFrame.Socket.Description.NetSender;

            try
            {
                connectFrame.Socket.TCPClient.EndConnect(ar);
                NetReceiver.OnConnectingResult(connectFrame.ReqID, desc, true, null);

                connectFrame.Socket.EnterIO();

                SocketAsyncEventArgs readEventArgs = IOManager.RetreiveEventArgs();// new SocketAsyncEventArgs();//= saeaPool.pop(); // saeaPool from the ASIOManager
                try
                {
                    readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    readEventArgs.UserToken = connectFrame.Socket;
                    bool willRaiseEvent = connectFrame.Socket.TCPClient.Client.ReceiveAsync(readEventArgs);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(readEventArgs);
                        // 위에서 releaseSocket이 되면, 아래 예외처리는 수행되지 않는다.
                    }
                }
                catch (Exception /*ex*/)
                {
                    // releasesocket을 수정하지 말고 각 문맥에서 releaseSAEA를 호출하자
                    if (null != readEventArgs)
                    {
                        readEventArgs.Completed -= IO_Completed;
                        IOManager.ReleaseSAEA(readEventArgs);
                    }
                    ReleaseSocket(connectFrame.Socket);
                }

            }
            catch (Exception ex)
            {
                lock (this)
                {
                    Sockets.Remove(desc.ManagedID);
                }
                NetReceiver.OnConnectingResult(connectFrame.ReqID, desc, false, ex);
                connectFrame.Socket.TCPClient = null;
            }
        }


        private void ReleaseSocket(AsyncSocket socket)
        {
            //TODO 구현이 좀 이상한듯
            lock (this)
            {
                int iocount = socket.ExitIO();

                if (0 == iocount)
                {
                    // 이 경우에는 Dispose()를 위해 Close()할 필요는 없다?
                    //socket.tcpclient.GetStream().Close();
                    //socket.tcpclient.Close();
                    //socket.sendlist.Clear();
                    // 객체파괴가 어떻게 일어날지 모르니, desc는 값을 복사해서 사용하자.
                    Sockets.Remove(socket.Description.ManagedID);
                    AsyncSocketContext desc = new AsyncSocketContext();
                    desc.UniqueId = socket.Description.UniqueId;
                    desc.ManagedID = socket.Description.ManagedID;
                    desc.NetSender = socket.Description.NetSender;
                    
                    // socket에 대해 메모리 풀링이 필요하다면, clone()과 아래 함수가 풀링에 대해 어떻게 처리할지 담당하면된다.
                    //  socket이 구체적으로 어떤 타입인지에 따라 풀링매니저가 달라져야 하기 때문에, asio계층에선 담당하지 않는다.
                    NetReceiver.OnReleaseSocket(desc, socket);

                }
            }
        }

    } // end Class
}
