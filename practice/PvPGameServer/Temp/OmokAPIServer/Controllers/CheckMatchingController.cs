using System;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Enums;
using OmokAPIServer.Models.Msg;
using OmokAPIServer.Models.Redis;
using OmokAPIServer.Options;
using OmokAPIServer.Services;

namespace OmokAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckMatchingController : SessionControllerBase<CheckMatchingController>
    {
        public CheckMatchingController(
            IOptions<ServerOption> serverOption, ILogger<CheckMatchingController> logger, ICacheService cacheService, IDBService dbService)
            : base(serverOption, logger, cacheService, dbService)
        {
        }
        
        [HttpPost]
        public async Task<ActionResult<ResCheckMatching>> PostCheckMatching(ReqCheckMatching reqCheckMatching)
        {
            // Cache에 세션 확인 및 갱신
            var (cacheResultCode, clientSession) = await UpdateSession(reqCheckMatching.Nickname, reqCheckMatching.AuthToken);
            if (cacheResultCode != ResultCode.Success)
            {
                return CreatedAtAction(nameof(PostCheckMatching),
                    new ResMatching
                    {
                        ResultCode = (uint) cacheResultCode,
                    });
            }
            
            // Cache에서 매칭 결과 찾기
            var key = KeyDefine.PrefixMatchingResult + reqCheckMatching.Nickname;
            var matchingResultBytes = await CacheService.GetAsync<byte[]>(key);
            if (matchingResultBytes != null)
            {
                var matchingResult = MessagePackSerializer.Deserialize<PvPMatchingResult>(matchingResultBytes);
                return CreatedAtAction(nameof(PostCheckMatching),
                    new ResCheckMatching()
                    {
                        ResultCode = (uint) ResultCode.Success,
                        IP = matchingResult.IP,
                        Port = matchingResult.Port,
                        RoomNumber = matchingResult.RoomNumber,
                        Index = matchingResult.Index,
                        SessionLastTime = clientSession.LastTime,
                        SessionExpireTime = clientSession.ExpireTime,
                        Token = matchingResult.Token,
                    });
            }
            else
            {
                return CreatedAtAction(nameof(PostCheckMatching),
                    new ResCheckMatching()
                    {
                        ResultCode = (uint) ResultCode.MatchingFail,
                        SessionLastTime = clientSession.LastTime,
                        SessionExpireTime = clientSession.ExpireTime,
                    });
            }
        }
    }
}