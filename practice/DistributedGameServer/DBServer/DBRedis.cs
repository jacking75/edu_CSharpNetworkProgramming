using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudStructures;
using CloudStructures.Structures;
using ServerCommon;

namespace DBServer
{
    public class DBRedis {

        public RedisConnection Connection { get; set; }
        
        public void Init(string address)
        {
            var config = new RedisConfig("db", address);
            Connection = new RedisConnection(config);
        }


        public SErrorCode IsSuccessGatewayLogin(string userID, string authToken)
        {
            var key = ServerCommon.Redis.RedisKeyDefs.GatewayUserAuthPrefix + userID;
            var redis = new RedisString<string>(Connection, key, null);
            var cachedObject = redis.GetAsync().Result;

            if(cachedObject.HasValue == false)
            {
                return SErrorCode.dbGatewayLoginInvalidUser;
            }

            if( cachedObject.Value != authToken )
            {
                return SErrorCode.dbGatewayLoginInvalidPW;
            }

            //TODO 로그인 성공인을 redis에 기록한다. 만약 이미 등록되어 있다면 중복 접속으르 판단한다
            // 만약 중복 접속이 아니라면 클라이언트에서 모든 접속 해제를 요청한다
            //개발 중에는 게이트웨이를 자주 종료해서 redis 데이터 삭제를 제대로 못할 수 있으므로 일단 주석으로
            /*var ret = CreateUserState(userID);
            if (ret != SErrorCode.None)
            {
                return ret;
            }*/

            return SErrorCode.None;
        }

        SErrorCode CreateUserState(string userID)
        {
            var key = ServerCommon.Redis.RedisKeyDefs.UserCurStatePrefix + userID;
            var redis = new RedisString<string>(Connection, key, TimeSpan.FromHours(6));
            var ret = redis.SetAsync("Login",
                            TimeSpan.FromHours(6), 
                            StackExchange.Redis.When.NotExists).Result;
            if(ret == false)
            {
                return SErrorCode.dbGatewayLoginDuplicate;
            }

            return SErrorCode.None;
        }

        public void GatewayLogout(string userID)
        {
            var key = ServerCommon.Redis.RedisKeyDefs.UserCurStatePrefix + userID;
            var redis = new RedisString<string>(Connection, key, null);
            redis.DeleteAsync().Wait();            
        }






    }
}
