using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmokAPIServer.Enums;
using OmokAPIServer.Models.Msg;
using OmokAPIServer.Models.Param;
using OmokAPIServer.Options;
using OmokAPIServer.Services;
using ZLogger;

namespace OmokAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : SessionControllerBase<LoginController>
    {
        public LoginController(
            IOptions<ServerOption> serverOption, ILogger<LoginController> logger, ICacheService cacheService, IDBService dbService)
            : base(serverOption, logger, cacheService, dbService)
        {
        }

        [HttpPost]
        public async Task<ActionResult<ResLogin>> Post(ReqLogin reqLogin)
        {
            // 닉네임 조건 추가
            if (reqLogin.Nickname.Length == 0)
            {
                return CreatedAtAction(nameof(Post),
                    new ResLogin
                    {
                        ResultCode = (uint) ResultCode.InvalidNickname,
                    });
            }
            
            // DB에 데이터가 있을 경우 Update, 없을 경우 Insert
            var (dbResultCode, isNew) = await UpdateUserToDbAsync(
                    reqLogin.Nickname, reqLogin.PlayerId, reqLogin.Did);
            
            if (dbResultCode != ResultCode.Success)
            {
                return CreatedAtAction(nameof(Post),
                    new ResLogin
                    {
                        ResultCode = (uint) dbResultCode,
                    });
            }

            // Cache에 세션 생성
            var (cacheResultCode, clientSession) = 
                    await CreateSession(reqLogin.Nickname, reqLogin.PlayerId);

            if (cacheResultCode != ResultCode.Success)
            {
                return CreatedAtAction(nameof(Post),
                    new ResLogin
                    {
                        ResultCode = (uint) cacheResultCode,
                    });
            }

            return CreatedAtAction(nameof(Post),
                new ResLogin
                {
                    ResultCode = (uint) ResultCode.Success,
                    SessionLastTime = clientSession.LastTime,
                    SessionExpireTime = clientSession.ExpireTime,
                    AuthToken = clientSession.AuthToken
                });
        }



        private async Task<Tuple<ResultCode, bool>> UpdateUserToDbAsync(string nickname, ulong playerId, string did)
        {
            try
            {
                DBService.Open();
                var result = await DBService.UpdateUserAsync(
                    new UpdateUserParam
                    {
                        Nickname = nickname,
                        PlayerId = playerId,
                        Did = did,
                        ClientIp = HttpContext.Connection.RemoteIpAddress.ToString()
                    });
                DBService.Close();
                return result;
            }
            catch (Exception e)
            {
                Logger.ZLogInformation(e.Message);
                DBService.Close();
                return new Tuple<ResultCode, bool>(ResultCode.DBCommonError, false);
            }
        }
    }
}