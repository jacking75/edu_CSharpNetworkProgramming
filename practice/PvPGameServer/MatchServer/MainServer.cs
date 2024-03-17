using System;
using System.Collections.Generic;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using Microsoft.Extensions.Options;

using System.Threading.Tasks;
using System.Threading;

// 매칭에 사용되는 게임서버들의 정보는 매칭 서버 실행전에 Redis의 매칭 요청 큐에 넣어둬야 한다.
// PvPGameServerInfoList


namespace MatchServer
{
    public class MainServer : IHostedService
    {                
        ServerOption ServerOpt;
        
        private readonly IHostApplicationLifetime AppLifetime;
        private readonly ILogger<MainServer> AppLogger;
                
        PvPMatchWorker MatchWorker = new ();

        Redis.RequestQueueWorker RedisReqWorker = new ();
        Redis.ResponseQueueWorker RedisResWorker = new ();
        

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
            AppLogger.LogInformation("OnStarted");

            Start(ServerOpt);            
        }

        private void AppOnStopped()
        {
            AppLogger.LogInformation("OnStopped - begin");

            Destory();

            AppLogger.LogInformation("OnStopped - end");
        }
                        
        void Start(ServerOption serverOpt)
        {
            AppLogger.LogInformation("Main Start - begin");
            
            // +----------------+    +-------------+    +----------------+
            // | RedisReqWorker | -> | MatchWorker | -> | RedisResWorker | 
            // +----------------+    +-------------+    +----------------+
            
            RedisReqWorker.SendToMatchingWorkerFunc = MatchWorker.AddMatchingRequest;
            RedisReqWorker.Start(AppLogger, serverOpt.RedisAddress);
            
            MatchWorker.SendToResponseWorkerFunc = RedisResWorker.AddMatchingResult;         
            MatchWorker.Start(AppLogger, serverOpt);
            
            if(RedisResWorker.Start(AppLogger, serverOpt.RedisAddress) == false)
            {
                return;
            }
                        
            AppLogger.LogInformation("Main Start - end");
        }

        void Destory()
        {
            AppLogger.LogInformation("Main Destory - begin");

            MatchWorker.Destroy();
            RedisResWorker.Destroy();
            RedisReqWorker.Destroy();
            
            AppLogger.LogInformation("Main Destory - begin");
        }

       
        
    }
}
