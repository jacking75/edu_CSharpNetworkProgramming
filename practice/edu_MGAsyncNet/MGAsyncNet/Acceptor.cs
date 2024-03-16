using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace MGAsyncNet
{    
    public class Acceptor
    {
        public enum eState
        {
            eState_None,
            eState_Run,
        }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public eState CurrentState { get; set; }
        
        TcpListener Listener;

        Thread ListenerThread;
        
        AsyncIOManager AsyncIOMgr;
        
        
        public Acceptor(AsyncIOManager asio, string ipAddress, int port)
        {
            AsyncIOMgr = asio;
            IPAddress = ipAddress;
            Port = port;
            CurrentState = eState.eState_None;
        }

        public bool Start()
        {
            if (eState.eState_None != CurrentState)
            {
                return false;
            }

            ListenerThread = new Thread(new ThreadStart(DoListen_tf));
            ListenerThread.Start();

            CurrentState = eState.eState_Run;
            return true;
        }

        // 스레드용 함수임을 표시하기 위해 이름 뒤에 _tf(thread function)를 붙임
        void DoListen_tf()
        { 
            try
            {
                if ("0.0.0.0" == IPAddress)
                {
                    Listener = new TcpListener(System.Net.IPAddress.Any, Port);
                }
                else
                {
                    Listener = new TcpListener(System.Net.IPAddress.Parse(IPAddress), Port);
                }

                Listener.Start();


                do
                {                    
                    AsyncIOMgr.registerSocket(Listener.AcceptTcpClient());

                } while (true);
            }
            catch (Exception /*ex*/)
            {
            }
        }

    } // end Class
}
