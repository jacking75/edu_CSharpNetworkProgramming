using System;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Enums;
using OmokAPIServer.Models;
using OmokAPIServer.Models.Msg;
using OmokAPIServer.Models.Redis;
using OmokAPIServer.Models.Table;
using OmokAPIServer.Options;
using OmokAPIServer.Services;
using ZLogger;

namespace OmokAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchingController : SessionControllerBase<MatchingController>
    {
        public MatchingController(
            IOptions<ServerOption> serverOption, ILogger<MatchingController> logger, ICacheService cacheService, IDBService dbService)
            : base(serverOption, logger, cacheService, dbService)
        {
        }
        
        [HttpPost]
        public async Task<ActionResult<ResMatching>> PostMatching(ReqMatching reqMatching)
        {
            // Cache에 세션 확인 및 갱신
            var (cacheResultCode, clientSession) = await UpdateSession(reqMatching.Nickname, reqMatching.AuthToken);
            if (cacheResultCode != ResultCode.Success)
            {
                return CreatedAtAction(nameof(PostMatching),
                    new ResMatching
                    {
                        ResultCode = (uint) cacheResultCode,
                    });
            }
            
            // DB 데이터 확인
            var userData = await GetUserFromDB(reqMatching.Nickname);
            if (userData == null)
            {
                return CreatedAtAction(nameof(PostMatching),
                    new ResMatching
                    {
                        ResultCode = (uint) ResultCode.UserNotExist,
                    });
            }

            // 매칭서버에 요청하기 위해 레디스에 추가
            var requestToRedis = new RequestPvPMatching
                {
                    UserID = userData.nickname,
                    RatingScore = 1000
                };
            
            var sendBytes = MessagePackSerializer.Serialize(requestToRedis);
            MsgPackHeaderInfo.WriteId(sendBytes, reqMatching.IsMatching ? (ushort)TaskId.RequestMatching : (ushort)TaskId.CancelMatching);
            var cacheResult = await CacheService.AddListAsync(KeyDefine.RequestMatchingQueue, sendBytes);
            if (!cacheResult)
            {
                return CreatedAtAction(nameof(PostMatching),
                    new ResMatching
                    {
                        ResultCode = (uint) ResultCode.CacheAddListError,
                    });
            }

            return CreatedAtAction(nameof(PostMatching),
                new ResMatching
                {
                    ResultCode = (uint) ResultCode.Success,
                    SessionLastTime = clientSession.LastTime,
                    SessionExpireTime = clientSession.ExpireTime,
                });
        }
        
        private async Task<User> GetUserFromDB(string nickname)
        {
            try
            {
                DBService.Open();
                var result = await DBService.GetUserAsync(
                    new GetUserParam
                    {
                        Nickname = nickname,
                    });
                DBService.Close();
                return result;
            }
            catch (Exception e)
            {
                Logger.ZLogInformation(e.Message);
                DBService.Close();
                return null;
            }
        }
    }
}