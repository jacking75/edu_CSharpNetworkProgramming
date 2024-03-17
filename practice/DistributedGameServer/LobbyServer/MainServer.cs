using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using Microsoft.Extensions.Options;

using System;
using System.Threading.Tasks;
using System.Threading;
using ServerCommon;


//TODO 로비의 상태(현재 접속 수)는 Redis에 저장한다.
// Redis 저장은 여기서 하지 않고, DB 서버에 요청한다.

namespace LobbyServer
{
    class MainServer : IHostedService
    {
        private readonly IHostApplicationLifetime AppLifetime;
        private readonly ILogger<MainServer> AppLogger;

        static readonly ILogger<MainServer> Logger = LogManager.GetLogger<MainServer>();

        ServerOption ServerOpt;

        LobbyManager LobbyMgr = new LobbyManager();

        MqManager MQMgr = new MqManager();

        PacketDistributor Distributor = new PacketDistributor();

        public static UInt16 ServerIdx;


        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, ILogger<MainServer> logger)
        {
            ServerOpt = serverConfig.Value;
            AppLogger = logger;
            AppLifetime = appLifetime;
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
            Logger.ZLogInformation("OnStarted");
            Start(ServerOpt);
        }

        private void AppOnStopped()
        {
            Logger.ZLogInformation("OnStopped - begin");

            Stop();

            Logger.ZLogInformation("OnStopped - end");
        }


        public SErrorCode Start(ServerOption serverOpt)
        {
            Logger.ZLogInformation("Start - begin");

            MQMgr.Init(serverOpt, ReceivedMQData);

            ServerIdx = serverOpt.ServerIndex;
                        
            Lobby.MQSendFunc = SendMqData;
            LobbyMgr.Init(serverOpt);
            

            PacketProcessor.MQSendFunc = SendMqData;
            Distributor.CreateAndStart(serverOpt, LobbyMgr);
            LobbyMgr.LobbyRegisterRedis();

            Logger.ZLogInformation("Start - end");
            return SErrorCode.None;
        }

      
        public void Stop()
        {
            Logger.ZLogInformation("Server Stop - begin");
            
            MQMgr.Destory();

            Distributor.Destory();

            Logger.ZLogInformation("Server Stop  - end");
        }


        void ReceivedMQData(int mqIndex, byte[] data)
        {
            Distributor.Distribute(mqIndex, data);
        }

        void SendMqData(int mqIndex, string subject, byte[] payload, int payloadLen)
        {
            MQMgr.Send(mqIndex, subject, payload, payloadLen);
        }


    } // end class
}
