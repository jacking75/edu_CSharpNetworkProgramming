using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using laster40Net.Util;

namespace laster40Net
{
    internal class Listener : IDisposable
    {
        /// <summary>
        /// 동작중인가?
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 중지 중인가?
        /// </summary>
        public bool IsStopping { get; private set; }

        private TcpService _service = null;
        private Thread _thread = null;
        private Socket _socket = null;
        private AutoResetEvent _connected = null;
        private AutoResetEvent _startupFinished = null;
        private AtomicInt _suspended = new AtomicInt();
        public IPEndPoint EndPoint { get; private set; }
        public int Backlog { get; private set; }


        public Listener(TcpService service, IPEndPoint endPoint, int backlog)
        {
            _service = service;
            EndPoint = endPoint;
            Backlog = backlog;
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if( disposing )
            {
                if (IsRunning)
                {
                    Stop();
                }

                _connected.Close();
                _connected = null;
            }

            _service = null;
            _thread = null;
        }
        #endregion


        public bool Start()
        {
            IsRunning = false;
            IsStopping = false;

            try
            {
                _connected = new AutoResetEvent(false);
                _startupFinished = new AutoResetEvent(false);
                _thread = new Thread(ListenThreadEntry);
                _thread.Start();
            }
            catch (Exception e)
            {
                _service.Logger.Log(LogLevel.Error, "listner 시작 실패", e);
                return false;
            }

            _startupFinished.WaitOne();

            return IsRunning;
        }

        public void Suspend(bool suspending)
        {
            if( suspending )
            {
                _suspended.On();
            }
            else
            {
                _suspended.Off();
            }
        }

        public void Stop()
        {
            IsStopping = true;

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            if (_thread != null )
            {
                _thread.Join();
                _thread = null;
            }

            // waitting for shutdown
            while (IsRunning)
            {
                const int TIMEOUT = 10;
                Thread.Sleep(TIMEOUT);
            }
        }
                
        /// <summary>
        /// listener 을 위한 스레드 1개
        /// </summary>
        private void ListenThreadEntry()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(EndPoint);
                _socket.Listen(Backlog);
            }
            catch (Exception e)
            {
                _service.Logger.Log(LogLevel.Error, e);

                // 시작 완료되었어요~ 근데 소켓 생성할때 에러가 있어서 시작을 못했네~
                _startupFinished.Set();

                return;
            }

            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedAcceptClient);

            IsRunning = true;

            // 시작 완료되었어요~
            _startupFinished.Set();

            while (!IsStopping)
            {
                bool pending = false;

                try
                {
                    eventArgs.AcceptSocket = null;
                    pending = _socket.AcceptAsync(eventArgs);
                }
                catch (Exception e)
                {
                    _service.Logger.Log(LogLevel.Error, e);
                    break;
                }

                if (!pending)
                {
                    CompletedAcceptClient(null, eventArgs);
                }

                //TODO 접속 완료까지 대기 하지 않도록 한다.
                // 접속이 하나 완료될때까지는 대기~ 완료된후 다시~ 
                _connected.WaitOne();
            }

            IsRunning = false;
        }

        private void CompletedAcceptClient(object obj, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket client = e.AcceptSocket;
                _connected.Set();

                if (_suspended.IsOn())
                {
                    client.Close();
                    return;
                }

                _service.CompletedAccept(false, client);
            }
            else
            {
                _connected.Set();
            }
        }
    }
}
