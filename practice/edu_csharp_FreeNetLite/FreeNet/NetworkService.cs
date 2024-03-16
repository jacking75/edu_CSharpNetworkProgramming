using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace FreeNet
{
    public class NetworkService<TMessageResolver> where TMessageResolver : IMessageResolver, new()
    {
        SocketAsyncEventArgsPool ReceiveEventArgsPool;
        SocketAsyncEventArgsPool SendEventArgsPool;
        

        public IPacketDispatcher PacketDispatcher { get; private set; }
                
        public SessionManager SessionMgr { get; private set; }

        public ServerOption ServerOpt { get; private set; }

        UInt64 SequenceId = 0;

        ReserveClosingProcess ReserveClosingProc = new ReserveClosingProcess();


        /// <summary>
        /// 로직 스레드를 사용하려면 use_logicthread를 true로 설정한다.
        ///  -> 하나의 로직 스레드를 생성한다.
        ///  -> 메시지는 큐잉되어 싱글 스레드에서 처리된다.
        /// 
        /// 로직 스레드를 사용하지 않으려면 use_logicthread를 false로 설정한다.
        ///  -> 별도의 로직 스레드는 생성하지 않는다.
        ///  -> IO스레드에서 직접 메시지 처리를 담당하게 된다.
        /// </summary>
        /// <param name="use_logicthread">true=Create single logic thread. false=Not use any logic thread.</param>
        public NetworkService(ServerOption serverOption, IPacketDispatcher userPacketDispatcher = null)
        {
            ServerOpt = serverOption;
            SessionMgr = new SessionManager();

            if (userPacketDispatcher == null)
            {
                PacketDispatcher = new DefaultPacketDispatcher();
            }
            else
            {
                PacketDispatcher = userPacketDispatcher;
            }
        }


        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        //
        public void Initialize()
        {
            // receive버퍼만 할당해 놓는다.
            // send버퍼는 보낼때마다 할당하든 풀에서 얻어오든 하기 때문에.
            //int pre_alloc_count = 1;
            //var totalBytes = ServerOpt.MaxConnectionCount * ServerOpt.ReceiveBufferSize * pre_alloc_count;
            //BufferManager buffer_manager = new BufferManager(totalBytes, ServerOpt.ReceiveBufferSize);
            //buffer_manager.InitBuffer();


            // receive버퍼만 할당해 놓는다.
            // send버퍼는 보낼때마다 할당하든 풀에서 얻어오든 하기 때문에.
            const int pre_alloc_count = 1;
            int argsCount = ServerOpt.MaxConnectionCount * pre_alloc_count;
            int argsBufferSize = ServerOpt.ClientReceiveBufferSize;
            ReceiveEventArgsPool = new SocketAsyncEventArgsPool();            
            ReceiveEventArgsPool.Init(SocketAsyncEventArgsPoolBufferMgrType.Concurrent, argsCount, argsBufferSize);

            SendEventArgsPool = new SocketAsyncEventArgsPool();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs arg;

            for (int i = 0; i < ServerOpt.MaxConnectionCount; i++)
            {
                // 더이상 UserToken을 미리 생성해 놓지 않는다.
                // 다수의 클라이언트에서 접속 -> 메시지 송수신 -> 접속 해제를 반복할 경우 문제가 생김.
                // 일단 on_new_client에서 그때 그때 생성하도록 하고,
                // 소켓이 종료되면 null로 세팅하여 오류 발생시 확실히 드러날 수 있도록 코드를 변경한다.

                // receive pool
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
                    arg.UserToken = null;

                    // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                    //buffer_manager.SetBuffer(arg);

                    // add SocketAsyncEventArg to the pool
                    ReceiveEventArgsPool.Push(arg);
                }


                // send pool
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(SendCompleted);
                    arg.UserToken = null;

                    // send버퍼는 보낼때 설정한다. SetBuffer가 아닌 BufferList를 사용.
                    arg.SetBuffer(null, 0, 0);

                    // add SocketAsyncEventArg to the pool
                    SendEventArgsPool.Push(arg);
                }
            }


            ReserveClosingProc.Start(ServerOpt.ReserveClosingWaitMilliSecond);
        }

        public void Stop()
        {
            ReserveClosingProc.Stop();
        }

        public void Listen(string host, int port, int backlog, SocketOption socketOption)
        {
            Listener client_listener = new Listener();
            client_listener.OnNewClientCallback += OnNewClient;
            client_listener.Start(host, port, backlog, socketOption);

            // heartbeat.
            byte check_interval = 10;
            SessionMgr.StartHeartbeatChecking(check_interval, check_interval);
        }

        /// <summary>
        /// 원격 서버에 접속 성공 했을 때 호출됩니다.
        /// </summary>
        /// <param name="socket"></param>
        public void OnConnectCompleted(Socket socket, Session token)
        {
            token.OnSessionClosed += OnSessionClosed;

            SessionMgr.Add(token);

            // SocketAsyncEventArgsPool에서 빼오지 않고 그때 그때 할당해서 사용한다.
            // 풀은 서버에서 클라이언트와의 통신용으로만 쓰려고 만든것이기 때문이다.
            // 클라이언트 입장에서 서버와 통신을 할 때는 접속한 서버당 두개의 EventArgs만 있으면 되기 때문에 그냥 new해서 쓴다.
            // 서버간 연결에서도 마찬가지이다.
            // 풀링처리를 하려면 c->s로 가는 별도의 풀을 만들어서 써야 한다.
            SocketAsyncEventArgs receive_event_arg = new SocketAsyncEventArgs();
            receive_event_arg.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
            receive_event_arg.UserToken = token;
            receive_event_arg.SetBuffer(new byte[1024], 0, 1024);

            SocketAsyncEventArgs send_event_arg = new SocketAsyncEventArgs();
            send_event_arg.Completed += new EventHandler<SocketAsyncEventArgs>(SendCompleted);
            send_event_arg.UserToken = token;
            send_event_arg.SetBuffer(null, 0, 0);

            BeginReceive(socket, receive_event_arg, send_event_arg);
        }

        // 스레드 세이프 하지 않다
        public UInt64 MakeSequenceIdForSession() { return ++SequenceId; }
        /// <summary>
        /// 새로운 클라이언트가 접속 성공 했을 때 호출됩니다.
        /// AcceptAsync의 콜백 매소드에서 호출되며 여러 스레드에서 동시에 호출될 수 있기 때문에 공유자원에 접근할 때는 주의해야 합니다.
        /// </summary>
        /// <param name="client_socket"></param>
        void OnNewClient(Socket client_socket, object token)
        {
            // UserToken은 매번 새로 생성하여 깨끗한 인스턴스로 넣어준다.
            var uniqueId = MakeSequenceIdForSession();
            var messageResolver = new TMessageResolver();
            messageResolver.Init(ServerOpt.ReceiveSecondaryBufferSize);
            var user_token = new Session(true, uniqueId, PacketDispatcher, messageResolver, ServerOpt);
            user_token.OnSessionClosed += OnSessionClosed;


            SessionMgr.Add(user_token);

            // 플에서 하나 꺼내와 사용한다.
            SocketAsyncEventArgs receive_args = this.ReceiveEventArgsPool.Pop();
            SocketAsyncEventArgs send_args = this.SendEventArgsPool.Pop();

            receive_args.UserToken = user_token;
            send_args.UserToken = user_token;


            user_token.OnConnected();

            BeginReceive(client_socket, receive_args, send_args);          
        }

        void BeginReceive(Socket socket, SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
        {
            // receive_args, send_args 아무곳에서나 꺼내와도 된다. 둘다 동일한 CUserToken을 물고 있다.
            Session token = receive_args.UserToken as Session;

            if (token == null || token.IsConnected() == false)
            {
                return;
            }

            token.SetEventArgs(receive_args, send_args);
            // 생성된 클라이언트 소켓을 보관해 놓고 통신할 때 사용한다.
            token.Sock = socket;

            bool pending = socket.ReceiveAsync(receive_args);
            if (!pending)
            {
                ProcessReceive(receive_args);
            }
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(e);
                return;
            }

            throw new ArgumentException("The last operation completed on the socket was not a receive.");
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed send operation</param>
        void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            Session token = e.UserToken as Session;

            if (token == null || token.IsConnected() == false)
            {
                return;
            }

            try
            {
                token.ProcessSend(e);
            }
            catch (Exception)
            {
                token.SetReserveClosing(ServerOpt.ReserveClosingWaitMilliSecond);
            }
        }

        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Session token = e.UserToken as Session;

            if (token == null || token.IsConnected() == false)
            {
                return;
            }
            ///////////////////////////////////////////

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                token.OnReceive(e.Buffer, e.Offset, e.BytesTransferred);

                // Keep receive.
                bool pending = token.Sock.ReceiveAsync(e);
                if (!pending)
                {
                    // Oh! stack overflow??
                    ProcessReceive(e);
                }
            }
            else
            {
                token.SetReserveClosing(ServerOpt.ReserveClosingWaitMilliSecond);

            }
        }

        public void OnSessionClosed(Session token)
        {
            SessionMgr.Remove(token);

            // Free the SocketAsyncEventArg so they can be reused by another client
            // 버퍼는 반환할 필요가 없다. SocketAsyncEventArg가 버퍼를 물고 있기 때문에
            // 이것을 재사용 할 때 물고 있는 버퍼를 그대로 사용하면 되기 때문이다.
            if (ReceiveEventArgsPool != null)
            {
                ReceiveEventArgsPool.Push(token.ReceiveEventArgs);
            }

            if (SendEventArgsPool != null)
            {
                SendEventArgsPool.Push(token.SendEventArgs);
            }

            token.SetEventArgs(null, null);
        }




    }
}
