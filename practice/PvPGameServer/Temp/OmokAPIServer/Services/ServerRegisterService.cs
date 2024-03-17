using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Models.Redis;
using OmokAPIServer.Options;
using ZLogger;

namespace OmokAPIServer.Services
{
    public class ServerRegisterService : IHostedService
    {
        readonly ILogger<ServerRegisterService> Logger;
        readonly ServerOption ServerOption;
        readonly ICacheService CacheService;
        
        public ServerRegisterService(IOptions<ServerOption> serverOption, ILogger<ServerRegisterService> logger,
            ICacheService cacheService)
        {
            ServerOption = serverOption.Value;
            Logger = logger;
            CacheService = cacheService;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (ServerOption.GameServerGroup != null)
            {
                CacheService.DeleteListAsync(KeyDefine.PvPGameServerList);
                var result = CacheService.SetAsync(KeyDefine.PvPGameServerList, ServerOption.GameServerGroup, 0);
                if (result.Result)
                {
                    Logger.ZLogInformation($"게임 서버 등록 성공: Count={ServerOption.GameServerGroup.ServerList.Count}");
                }
                else
                {
                    Logger.ZLogInformation($"게임 서버 등록 실패");
                }
            }
            else
            {
                Logger.ZLogInformation($"게임 서버 데이터 파싱 실패");
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}