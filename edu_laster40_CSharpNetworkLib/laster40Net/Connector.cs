using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using laster40Net.Util;

namespace laster40Net
{
    internal class Connector
    {
        private TcpService _service = null;
        private Thread _worker;
        private AtomicInt _cancel = new AtomicInt();
        private TcpServiceConfig.ClientConfig _config;
        private Object _token;
        public System.Net.IPEndPoint EndPoint { get; set; }

        public Connector(TcpService service, TcpServiceConfig.ClientConfig config, Object token)
        {
            _service = service;
            _config = config;
            _token = token;

            System.Net.IPAddress addr;
            System.Net.IPAddress.TryParse(_config.ip, out addr);
            EndPoint = new System.Net.IPEndPoint(addr, _config.port);
        }

        public bool Start()
        {
            _worker = new Thread(ThreadEntry);
            try
            {
                _worker.Start();
            }
            catch(Exception )
            {
                return false;
            }

            return true;
        }

        public void Stop()
        {
            Cancel();

            if (_worker != null)
            {
                _worker.Join();
                _worker = null;
            }
        }

        public bool Cancel()
        {
            _cancel.On();
            return true;
        }

        private Socket ConnectToserver(IPEndPoint endPoint, int timeoutMs)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Blocking = false;
                socket.Connect(endPoint);
                return socket;
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode != 10035)
                {
                    socket.Close();
                    return null;
                }
            }

            try
            {
                int timeoutMicroseconds = timeoutMs * 1000;
                if (socket.Poll(timeoutMicroseconds, SelectMode.SelectWrite) == false)
                {
                    socket.Close();
                    return null;
                }

                socket.Blocking = true;
            }
            catch(Exception /*e*/)
            {
                return null;
            }

            return socket;
        }
        
        private void ThreadEntry()
        {
            int retryCount = 0;
            while(!_cancel.IsOn())
            {
                if( _config.retry > 0 )
                {
                    if( retryCount++ > _config.retry )
                        break;
                }
               
                Socket socket = ConnectToserver(EndPoint, _config.timeout);
                if (socket != null)
                {
                    if (_cancel.IsOn())
                    {
                        socket.Close();
                        break;
                    }

                    _service.CompletedConnect(true, EndPoint, socket, _token);
                    return;
                }

                Thread.Sleep(50);
            }

            _service.CompletedConnect(false, EndPoint, null, _token);
        }
    }
}
