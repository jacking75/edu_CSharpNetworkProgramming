using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Enums;
using OmokAPIServer.Models.Msg;
using OmokAPIServer.Options;
using OmokAPIServer.Services;

namespace OmokAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UpdateSessionController : SessionControllerBase<UpdateSessionController>
    {
        public UpdateSessionController(
            IOptions<ServerOption> serverOption, ILogger<UpdateSessionController> logger, ICacheService cacheService, IDBService dbService)
            : base(serverOption, logger, cacheService, dbService)
        {
        }

        [HttpPost]
        public async Task<ActionResult<ResUpdateSession>> PostLogin(ReqUpdateSession reqUpdateSession)
        {
            // Cache에 세션 갱신
            var (cacheResultCode, clientSession) = 
                await UpdateSession(reqUpdateSession.Nickname, reqUpdateSession.AuthToken);

            if (cacheResultCode != ResultCode.Success)
            {
                return CreatedAtAction(nameof(PostLogin),
                    new ResUpdateSession
                    {
                        ResultCode = (uint) cacheResultCode,
                    });
            }
            
            return CreatedAtAction(nameof(PostLogin),
                new ResUpdateSession
                {
                    ResultCode = (uint) ResultCode.Success,
                    SessionLastTime = clientSession.LastTime,
                    SessionExpireTime = clientSession.ExpireTime
                });
        }
    }
}