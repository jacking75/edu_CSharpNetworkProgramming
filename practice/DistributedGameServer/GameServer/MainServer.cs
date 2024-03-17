using System;
using System.Collections.Generic;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Threading.Tasks;
using System.Threading;
using PvPGameServer.Enum;


namespace PvPGameServer
{
    public class MainServer : AppServer<NetworkSession, EFBinaryRequestInfo>, IHostedService
    {
        public static ILog GlobalLogger;
                
        PacketProcessor MainPacketProcessor = new PacketProcessor();

        ServerOption ServerOpt;
        IServerConfig ServerCfg;

        private readonly IHostApplicationLifetime AppLifetime;
        private readonly ILogger<MainServer> AppLogger;

        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverOption, ILogger<MainServer> logger)
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            ServerOpt = serverOption.Value;
            AppLogger = logger;
            AppLifetime = appLifetime;

            NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
            SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            AppLifetime.ApplicationStarted.Register(AppOnStarted);
            AppLifetime.ApplicationStopped.Register(AppOnStopped);
                        
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void AppOnStarted()
        {
            AppLogger.LogInformation("OnStarted");

            InitConfig(ServerOpt);
            
            CreateServer(ServerOpt);

            var bResult = base.Start();
            if (bResult)
            {
                AppLogger.LogInformation("서버 네트워크 시작");
            }
            else
            {
                AppLogger.LogError("서버 네트워크 시작 실패");
            }
        }

        private void AppOnStopped()
        {
            GlobalLogger.Info("OnStopped - begin");

            base.Stop();

            MainPacketProcessor.Destroy();

            GlobalLogger.Info("OnStopped - end");
        }
                
        void InitConfig(ServerOption option)
        {
            ServerCfg = new ServerConfig
            {
                Name = option.Name,

                Port = option.Port,
                Ip = "Any",
                MaxConnectionNumber = option.MaxConnectionNumber,
                Mode = SocketMode.Tcp,
                MaxRequestLength = option.MaxRequestLength, 
                ReceiveBufferSize = option.ReceiveBufferSize,
                SendBufferSize = option.SendBufferSize
            };
        }

        void CreateServer(ServerOption serverOpt)
        {
            try
            {
                bool bResult = Setup(new RootConfig(), ServerCfg, logFactory: new NLogLogFactory());
                if (bResult == false)
                {
                    GlobalLogger.Error("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
                    return;
                }
                else
                {
                    GlobalLogger = base.Logger;
                }

                
                MainPacketProcessor.NetSendFunc = this.SendData;
                MainPacketProcessor.DistributePacketFunc = this.Distribute;
                MainPacketProcessor.ForcedCloseSessionFunc = this.ForcedCloseSession;
                MainPacketProcessor.CreateAndStart(serverOpt);

                GlobalLogger.Info($"[서버 생성 성공] Index: {serverOpt.ServerUniqueID}, Port:{serverOpt.Port}");
            }
            catch(Exception ex)
            {
                GlobalLogger.Error($"[ERROR] 서버 생성 실패: {ex}");
            }
        }
        
        bool SendData(string sessionID, byte[] sendData)
        {
            var session = GetSessionByID(sessionID);
            if (session == null)
            {
                return false;
            }
            
            try
            {
                session.Send(sendData, 0, sendData.Length);
            }
            catch (Exception ex)
            {
                // TimeoutException 예외가 발생할 수 있다
                GlobalLogger.Error($"{ex.Message},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        public bool ForcedCloseSession(string sessionID)
        {
            var session = GetSessionByID(sessionID);
            if (session == null)
            {
                return false;
            }

            session.Close();
            return true;
        }

        void Distribute(EFBinaryRequestInfo requestPacket)
        {
            MainPacketProcessor.InsertPacket(requestPacket);
        }

        void OnConnected(NetworkSession session)
        {
            // 옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConnected 함수가 호출되지 않는다
            GlobalLogger.Info(string.Format($"세션 번호 {session.SessionID} 접속"));

            SendNtfInnerConnectOrDisConnectClientPacket(true, session.SessionID);
        }

        void OnClosed(NetworkSession session, CloseReason reason)
        {
            GlobalLogger.Info(string.Format($"세션 번호 {session.SessionID} 접속해제: {reason.ToString()}"));

            SendNtfInnerConnectOrDisConnectClientPacket(false, session.SessionID);
        }

        void OnPacketReceived(NetworkSession session, EFBinaryRequestInfo reqInfo)
        {
            GlobalLogger.Debug(string.Format($"세션 번호 {session.SessionID} 받은 데이터 크기: {reqInfo.Body.Length}, ThreadId: {Thread.CurrentThread.ManagedThreadId}"));

            var packetId = (PacketID)MsgPackPacketHeaderInfo.ReadPacketID(reqInfo.Data);
            if(packetId <= PacketID.CS_BEGIN || packetId >= PacketID.CS_END)
            {
                GlobalLogger.Error($"Disable Send Packet From Client. SessionID:{session.SessionID}, PacketId: {packetId}");
                return;
            }

            reqInfo.SessionID = session.SessionID;       
            Distribute(reqInfo);         
        }
      
        void SendNtfInnerConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
        {
            var packet = new EFBinaryRequestInfo(null);
            packet.Data = new byte[MsgPackPacketHeaderInfo.HeadSize];
            packet.SessionID = sessionID;

            if (isConnect)
            {
                MsgPackPacketHeaderInfo.WritePacketID(packet.Data, (UInt16)PacketID.NTF_IN_CONNECT_CLIENT);
            }
            else
            {
                MsgPackPacketHeaderInfo.WritePacketID(packet.Data, (UInt16)PacketID.NTF_IN_DISCONNECT_CLIENT);
            }

            Distribute(packet);
        }
    }

    public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>
    {
    }
}
