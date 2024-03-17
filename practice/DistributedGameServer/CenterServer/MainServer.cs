using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ServerCommon;

namespace CenterServer
{
    class MainServer : IHostedService
    {
        private readonly IHostApplicationLifetime AppLifetime;

        static readonly ILogger<MainServer> GZLogger = LogManager.GetLogger<MainServer>();

        ServerOption ServerOpt;

        MqManager MQMgr = new();

        PacketDistributor Distributor = new ();

        public static UInt16 ServerIdx;


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
            GZLogger.ZLogInformation("[OnStarted]");

            Start(ServerOpt);
        }

        private void AppOnStopped()
        {
            GZLogger.ZLogInformation("[OnStopped] - begin");

            Stop();

            GZLogger.ZLogInformation("[OnStopped] - end");
        }


        public SErrorCode Start(ServerOption serverOpt)
        {
            MQMgr.Init(serverOpt.ServerIndex, 
                serverOpt.MQServerAddressList,
                ServerCommon.MQ.SubjectManager.ToCenterServer(), 
                ReceivedMQData);

            ServerIdx = serverOpt.ServerIndex;

            Distributor.SendMqDataDelegate = SendMqData;
            Distributor.CreateAndStart(serverOpt);

            return SErrorCode.None;
        }


        public void Stop()
        {
            GZLogger.ZLogInformation("[Stop] - begin");

            MQMgr.Destory();

            Distributor.Destory();

            GZLogger.ZLogInformation("[Stop]  - end");
        }


        void ReceivedMQData(int index, byte[] data)
        {
            Distributor.Distribute(index, data);
        }

        void SendMqData(Int32 mqIndex, string subject, byte[] data, int dataLen)
        {
            MQMgr.Send(mqIndex, subject, data, dataLen);
        }
    }
}
