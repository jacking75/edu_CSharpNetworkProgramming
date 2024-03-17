using ServerCommon.Redis;

using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatchServer.Redis
{
    public class RequestQueueWorker
    {
        public Action<byte[]> SendToMatchingWorkerFunc;


        RedisConnection Connection;

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        ILogger Logger;


        public void Start(ILogger logger, string redisAddress)
        {
            Logger = logger;

            var config = new RedisConfig("input", redisAddress);
            Connection = new RedisConnection(config);
            
            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();

            Logger.LogInformation($"RequestQueueWorker Started. address:{redisAddress}");
        }

        public void Destroy()
        {
            Logger.LogInformation("RequestQueueWorker::Destroy - begin");

            IsThreadRunning = false;
            
            ProcessThread.Join();

            Logger.LogInformation("RequestQueueWorker::Destroy - end");
        }

        void Process()
        {
            var key = KeyDefine.RequestMatchingQueue;
            var redis = new RedisList<byte[]>(Connection, key, null);

            while (IsThreadRunning)
            {
                try
                {
                    Process_impl(redis);
                }
                catch (Exception ex)
                {
                    if (IsThreadRunning)
                    {
                        Logger.LogError(ex.ToString());
                    }
                }
            }          
        }

        void Process_impl(RedisList<byte[]> redis)
        {
            var reqList = redis.RangeAsync(0, -1).Result;
            if (reqList.Length < 1)
            {
                System.Threading.Thread.Sleep(10);
                return;
            }
                
            foreach (var request in reqList)
            {
                Logger.LogDebug("Request to PvPMatchingWorker");
                SendToMatchingWorkerFunc(request);
            }

            var nextReadPos = reqList.Length;
            redis.TrimAsync(nextReadPos, -1);            
        }

    }
}
