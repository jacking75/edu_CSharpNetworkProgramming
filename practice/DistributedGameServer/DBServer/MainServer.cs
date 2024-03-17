using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ServerCommon;

namespace DBServer
{
    class MainServer : IHostedService
    {
        private readonly IHostApplicationLifetime AppLifetime;
        
        public static readonly ILogger<MainServer> Logger = LogManager.GetLogger<MainServer>();

        ServerOption ServerOpt;

        MqManager MQMgr = new MqManager();
        
        PacketProcessManager PacketProcessMgr = new PacketProcessManager();


        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, ILogger<MainServer> logger)
        {
            ServerOpt = serverConfig.Value;
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
            Logger.ZLogInformation(new EventId(LogEventID.DBProgramInit), "OnStarted");
            
            Start(ServerOpt);
        }

        private void AppOnStopped()
        {
            Logger.ZLogInformation(new EventId(LogEventID.DBProgramEnd), "OnStopped - begin");

            Stop();

            Logger.ZLogInformation(new EventId(LogEventID.DBProgramEnd), "OnStopped - end");
        }



        public void Start(ServerOption serverOpt)
        {
            MQMgr.Init(serverOpt, ReceivedMQData);
            
            PacketProcessMgr.Init(serverOpt, MQMgr.Send);
        }


        public void Stop()
        {
            Logger.ZLogInformation(new EventId(LogEventID.DBProgramEnd), "Server Stop <<<<");

            PacketProcessMgr.Destory();

            MQMgr.Destory();

            Logger.ZLogInformation(new EventId(LogEventID.DBProgramEnd), "Server Stop >>>");
        }


        void ReceivedMQData(int index, byte[] data)
        {
            PacketProcessMgr.Distribute(index, data);
        }


    }
}
