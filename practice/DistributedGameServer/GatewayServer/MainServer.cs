using ServerCommon;
using MQ = ServerCommon.MQ;

using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

using ZLogger;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


//TODO: 프로젝트에 따라서 크게 달라질 부분이라서 작업 미완료. 빌드 안됨

namespace GatewayServer
{
    public class MainServer : AppServer<NetworkSession, GWBinaryRequestInfo>, IHostedService
    {
        private readonly IHostApplicationLifetime AppLifetime;
        
        static readonly ILogger<MainServer> GZLogger = LogManager.GetLogger<MainServer>();

        ServerOption ServerOpt;
        IServerConfig NetLibConfig;

        public static UInt16 Index { get; private set; }
        
        ConnSession.Manager ConnSessionMgr = new ConnSession.Manager();

        CSPacket.Handler CSPacketHndler = new CSPacket.Handler();

        MQPacketProcessor S2SPacketProcessor = new ();

        MQSubjectManager MQSubjectMgr = new MQSubjectManager();
        ServerCommon.MQ.Receiver MQReceiver;
        ServerCommon.MQ.Sender MQSender;
         

        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, ILogger<MainServer> logger)
            : base(new DefaultReceiveFilterFactory<RecvFilter, GWBinaryRequestInfo>())
        {
            ServerOpt = serverConfig.Value;
            AppLifetime = appLifetime;

            NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
            SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<NetworkSession, GWBinaryRequestInfo>(OnPacketReceived);
        }

        #region Generic Hosting
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
            GZLogger.ZLogInformation("OnStarted - begin");

            Initalize(ServerOpt);

            if(base.Start() == false)
            {
                GZLogger.ZLogError($"서버 네트워크 시작 실패. error: {SErrorCode.gwFailSuperSocketStart}");
                return;
            }


            MQSubjectMgr.RequestLobbyRoomMQInfo(ServerOpt.ServerUniqueID, MQSender.Send);
            GZLogger.ZLogInformation("[RequestLobbyRoomMQInfo]");

            GZLogger.ZLogInformation("OnStarted - end");            
        }

        private void AppOnStopped()
        {
            GZLogger.ZLogInformation("OnStopped - begin");

            StopServer();

            GZLogger.ZLogInformation("OnStopped - end");
        }
        #endregion

        SErrorCode Initalize(ServerOption serverOpt)
        {
            GZLogger.ZLogInformation("[Initalize] - begin");
            
            InitServerConfig(serverOpt);
                        
            if(base.Setup(new RootConfig(), NetLibConfig, logFactory: new NLogLogFactory()) == false)
            {
                GZLogger.ZLogError("[Initalize] - 서버 네트워크 설정 실패");
                return SErrorCode.gwFailInitSetup;
            }                       
            

            InitMQManager(serverOpt.MQServerAddress,
                            MQ.SubjectManager.ToGatewayServer(serverOpt.ServerUniqueID));


            CSPacket.Handler.SendNetworkFunc = SendPacket;
            CSPacket.Handler.SendMQToLobbyFunc = SendMQToLobby;
            CSPacket.Handler.SendMQToGameFunc = SendMQToGame;
            CSPacket.Handler.SendMQToDBFunc = SendMQToDB;

            CSPacketHndler.Init(serverOpt.ServerUniqueID, ConnSessionMgr);


            S2SPacketProcessor.S2CSendPacketFunc = SendPacket;
            S2SPacketProcessor.SendMQToLobby = SendMQToLobby;
            S2SPacketProcessor.SendMQToGame = SendMQToGame;
            S2SPacketProcessor.Init(ConnSessionMgr, MQSubjectMgr);


            GZLogger.ZLogInformation("[Initalize] - end");
            return SErrorCode.None;
        }
       
        public void StopServer()
        {
            base.Stop();

            S2SPacketProcessor.Destory();
            MQReceiver.Destory();
            MQSender.Destory();
        }

        void InitServerConfig(ServerOption serverOpt) 
        {
            NetLibConfig = new ServerConfig()
            {
                Name                    = serverOpt.Name,
                Ip                      = "Any",
                Port                    = serverOpt.Port,
                Mode                    = SocketMode.Tcp,
                MaxConnectionNumber     = serverOpt.MaxConnectionNumber,
                MaxRequestLength        = serverOpt.MaxRequestLength,
                ReceiveBufferSize       = serverOpt.ReceiveBufferSize,
                SendBufferSize          = serverOpt.SendBufferSize
            };

            Index = ServerOpt.ServerUniqueID;
        }

        void InitMQManager(string serverAddress, string subject)
        {
            MQReceiver = new ServerCommon.MQ.Receiver();
            MQReceiver.Init(0, serverAddress, subject, null);
            MQReceiver.ReceivedMQData = ReceiveMQ;

            MQSender = new ServerCommon.MQ.Sender();
            MQSender.Init(serverAddress);
        }

        #region Network Event Handler
        void OnConnected(NetworkSession session)
        {
            //로드밸런스에 의해 클라이언트가 접속 되므로 최대 연결 수 이상의 접속이 발생했을 때에 대해서는 고려하지 않는다.
            GZLogger.ZLogDebug($"[OnConnected] 세션: {session.SessionID}");
                        
            ConnSessionMgr.Add(session.SessionID);            
        }

        void OnClosed(NetworkSession netSession, CloseReason reason)
        {
            GZLogger.ZLogDebug($"[OnClosed] 세션: {netSession.SessionID}, reason: {reason.ToString()}");

            UInt64 uid = 0;

            //TODO 상태에 따른 연결 종료 처리하기
            var connSession = ConnSessionMgr.GetSession(netSession.SessionID);
            if (connSession != null)
            {
                uid = connSession.UniqueID;
                ConnSessionClosed(connSession);
            }

            ConnSessionMgr.Remove(netSession.SessionID, uid);            
        }

        void OnPacketReceived(NetworkSession session, GWBinaryRequestInfo reqInfo)
        {
            GZLogger.ZLogDebug($"[OnPacketReceived] 세션 번호: {session.SessionID} , 받은 데이터 크기: {reqInfo.Body.Length} , ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                        
            reqInfo.SessionID = session.SessionID;
            CSPacketHndler.Process(reqInfo);
        }
        #endregion


        void ConnSessionClosed(ConnSession.Session session)
        {
            if(session.IsCertified())
            {
                var (len, mqPacket) = session.MakeNotifyGatewayLogOut(Index, session.UserID);
                SendMQToDB(mqPacket, 0, len);
            }

            if(session.LobbyNum >= 0)
            {
                var (len, mqPacket) = session.MakeRequestLobbyLeaveMQPacket(Index, true);
                SendMQToLobby(session.LobbyNum, mqPacket, 0, len);
            }

            //TODO 게임서버까지는 구현하지 않았으므로 이 부분의 처리는 아직 없다
        }

        public bool SendPacket(string sessionID, byte[] packet)
        {
            var session = GetSessionByID(sessionID);

            try
            {
                if (session == null)
                {
                    return false;
                }

                session.Send(packet, 0, packet.Length);
            }
            catch (Exception ex)
            {
                // TimeoutException 예외가 발생할 수 있다
                GZLogger.ZLogError($"[SendPacket] {ex.ToString()}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }
                            
        public void SendMQToLobby(Int16 lobbyNum, byte[] packet, int offset, int count)
        {
            var subject = MQSubjectMgr.GetSubjectLobby(lobbyNum);
            if(string.IsNullOrEmpty(subject))
            {
                GZLogger.ZLogError($"[SendMQToLobby] lobbyNum:{lobbyNum}");
                return;
            }
            MQSender.Send(subject, packet, offset, count);           
        }

        public void SendMQToGame(Int32 roomNum, byte[] packet, int offset, int count)
        {
            var subject = MQSubjectMgr.GetSubjectRoom(roomNum);
            if (string.IsNullOrEmpty(subject))
            {
                GZLogger.ZLogError($"[SendMQToGame] roomNum:{roomNum}");
                return;
            }
            MQSender.Send(subject, packet);
        }

        public void SendMQToDB(byte[] packet, int offset, int count)
        {            
            MQSender.Send("to.DB", packet, offset, count);
        }

        public void ReceiveMQ(int mqIndex, byte[] packet)
        {
            S2SPacketProcessor.Distribute(packet);
        }
    }


    public class NetworkSession : AppSession<NetworkSession, GWBinaryRequestInfo>
    {        
    }
}
