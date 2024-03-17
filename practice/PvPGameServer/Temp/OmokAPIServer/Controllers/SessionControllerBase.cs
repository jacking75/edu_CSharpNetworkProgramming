using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Enums;
using OmokAPIServer.Models;
using OmokAPIServer.Options;
using OmokAPIServer.Services;
using MathNet.Numerics.Random;
using ZLogger;

namespace OmokAPIServer.Controllers
{
    public class SessionControllerBase<TController> : ControllerBase
    {
        protected readonly ILogger<TController> Logger;
        protected readonly ServerOption ServerOption;
        protected readonly IDBService DBService;
        protected readonly ICacheService CacheService;

        public SessionControllerBase(
            IOptions<ServerOption> serverOption, ILogger<TController> logger, ICacheService cacheService, IDBService dbService)
        {
            ServerOption = serverOption.Value;
            Logger = logger;
            CacheService = cacheService;
            DBService = dbService;
        }
        
        // 세션 생성
        protected async Task<Tuple<ResultCode, ClientSession>> CreateSession(string nickname, ulong playerId)
        {
            var sessionLastTime = DateTime.UtcNow;
            var sessionExpireTime = sessionLastTime.AddSeconds(ServerOption.SessionExpireTime);

            var session = new ClientSession
            {
                Nickname = nickname,
                PlayerId = playerId,
                LastTime = sessionLastTime.ToString(CultureInfo.InvariantCulture),
                ExpireTime = sessionExpireTime.ToString(CultureInfo.InvariantCulture),
                AuthToken = MakeAuthToken()
            };

            var isSuccess = await CacheService.SetAsync(nickname, session, (int)ServerOption.SessionExpireTime);
            if (!isSuccess)
            {
                return new Tuple<ResultCode, ClientSession>(ResultCode.CacheSetError, null);
            }
            
            return new Tuple<ResultCode, ClientSession>(ResultCode.Success, session);
        }
        
        // 세션 갱신
        protected async Task<Tuple<ResultCode, ClientSession>> UpdateSession(string nickname, string authToken)
        {
            var session = await CacheService.GetAsync<ClientSession>(nickname);
            if (session == null)
            {
                return new Tuple<ResultCode, ClientSession>(ResultCode.SessionNotExist, null);
            }
            else
            {
                if (session.AuthToken != authToken)
                {
                    return new Tuple<ResultCode, ClientSession>(ResultCode.InvalidAuthToken, null);
                }
                else if (DateTime.UtcNow > DateTime.ParseExact(session.ExpireTime, "MM/dd/yyyy HH:mm:ss", null))
                {
                    return new Tuple<ResultCode, ClientSession>(ResultCode.SessionTimeout, null);
                }
                else
                {
                    var sessionLastTime = DateTime.UtcNow;
                    var sessionExpireTime = sessionLastTime.AddSeconds(ServerOption.SessionExpireTime);
                    session.LastTime = sessionLastTime.ToString(CultureInfo.InvariantCulture);
                    session.ExpireTime = sessionExpireTime.ToString(CultureInfo.InvariantCulture);
                }
            }
            
            var isSuccess = await CacheService.SetAsync(nickname, session, (int)ServerOption.SessionExpireTime);
            if (!isSuccess)
            {
                return new Tuple<ResultCode, ClientSession>(ResultCode.CacheSetError, null);
            }
            
            return new Tuple<ResultCode, ClientSession>(ResultCode.Success, session);
        }
        private string MakeAuthToken()
        {
            var sb = new StringBuilder();
            var samples = SystemRandomSource.Default.NextInt32s(ServerOption.AuthTokenLength, 0, 15);
            for (var i = 0; i < samples.Length; i++)
            {
                sb.Append(samples[i].ToString("x1"));
            }

            return sb.ToString();
        }
    }
}