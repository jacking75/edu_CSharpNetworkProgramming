using System;
using System.Threading.Tasks;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Options;
using ZLogger;
using MessagePack;
using OmokAPIServer.Models.Redis;

namespace OmokAPIServer.Services
{
    public class CacheService : ICacheService
    {
        private readonly ILogger<CacheService> Logger;
        private RedisConnection Connection { get; }

        public CacheService(ILogger<CacheService> logger, IOptions<ServerOption> serverOption)
        {
            var config = new RedisConfig("redis", serverOption.Value.CacheConnStr);
            Connection = new RedisConnection(config);

            Logger = logger;
        }

        public async Task<TObj> GetAsync<TObj>(string key)
        {
            try
            {
                var redis = new RedisString<TObj>(Connection, key, null);
                var result = await redis.GetAsync();
                return result.HasValue ? result.Value : default;
            }
            catch (Exception e)
            {
                Logger.ZLogInformation(e.ToString());
                return default;
            }
        }
        
        public async Task<bool> SetAsync<TObj>(string key, TObj obj, int expireSec)
        {
            try
            {
                RedisString<TObj> redis;
                if (expireSec > 0)
                {
                    redis = new RedisString<TObj>(Connection, key, TimeSpan.FromSeconds(expireSec));    
                }
                else
                {
                    redis = new RedisString<TObj>(Connection, key, null);
                }
                return await redis.SetAsync(obj);
            }
            catch (Exception e)
            {
                Logger.ZLogInformation(e.Message);
                return false;
            }
        }

        public async Task<bool> AddListAsync(string key, byte[] bytes)
        {
            try
            {
                var redis = new RedisList<byte[]>(Connection, key, null);
                var result = await redis.RightPushAsync(bytes);
                return result > 0;
            }
            catch (Exception e)
            {
                Logger.ZLogInformation(e.Message);
                return false;
            }
        }
        public async Task<bool> DeleteListAsync(string key)
        {
            try
            {
                var redis = new RedisList<byte[]>(Connection, key, null);
                var result = await redis.DeleteAsync();
                return result;
            }
            catch (Exception e)
            {
                Logger.ZLogInformation(e.Message);
                return false;
            }
        }
    }
}