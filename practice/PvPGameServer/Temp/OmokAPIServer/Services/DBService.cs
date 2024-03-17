using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using OmokAPIServer.Enums;
using OmokAPIServer.Models.Param;
using OmokAPIServer.Models.Table;
using OmokAPIServer.Options;
using Dapper.Contrib.Extensions;
using OmokAPIServer.Models;

namespace OmokAPIServer.Services
{
    public class DBService : IDBService
    {
        private readonly IOptions<ServerOption> ServerOption;
        private readonly int DefaultTimeout;
        private IDbConnection DBConn;
        private IDbTransaction DBTransaction;
        private readonly ILogger<DBService> Logger;
        public DBService(IOptions<ServerOption> serverOption, ILogger<DBService> logger)
        {
            ServerOption = serverOption;
            Logger = logger;
            DefaultTimeout = serverOption.Value.DBTimeout;
        }
        // DB 열기.
        public void Open()
        {
            DBConn ??= new MySqlConnection(ServerOption.Value.DBConnStr);
            DBConn.Open();
            
            // 시간대를 UTC로 변경한다.
            DBConn.Execute("SET @@session.time_zone = 'UTC' ", null, DBTransaction, DefaultTimeout);
        }
        
        // DB 닫기.
        public void Close()
        {
            DBConn?.Close();
        }
        
        // 트랜잭션 시작.
        public void StartTransaction()
        {
            if (DBConn == null)
            {
                throw new Exception("DB is not opened");
            }
            if (DBTransaction != null)
            {
                throw new Exception("DB transaction is not finished");
            }

            DBTransaction = DBConn.BeginTransaction(IsolationLevel.RepeatableRead);

            if (DBTransaction == null)
            {
                throw new Exception("DB transaction error");
            }
        }
        
        // 트랜잭션 롤백.
        public void Rollback()
        {
            if (DBConn == null)
            {
                throw new Exception("DB is not opened");
            }
            if (DBTransaction == null)
            {
                throw new Exception("DB transaction is not started");
            }
            
            DBTransaction.Rollback();
            DBTransaction = null;
        }
        
        // 트랜잭션 커밋.
        public void Commit()
        {
            if (DBConn == null)
            {
                throw new Exception("DB is not opened");
            }
            if (DBTransaction == null)
            {
                throw new Exception("DB transaction is not started");
            }

            DBTransaction.Commit();
            DBTransaction = null;
        }
        
        // 유저 데이터 업데이트
        public async Task<Tuple<ResultCode, bool>> UpdateUserAsync(UpdateUserParam param)
        {
            if (DBConn == null)
            {
                throw new Exception("DB is not opened");
            }
            
            // 데이터가 이미 존재하는지 확인
            var iEnumerable = await DBConn.QueryAsync<User>(User.SelectQueryOne, new
            {
                nickname = param.Nickname,
            }, DBTransaction, DefaultTimeout);
            
            var playerDataList = iEnumerable.ToList();
            
            bool isNew;
            ResultCode resultCode;
            if (!playerDataList.Any())
            {
                isNew = true;
                var lastInsertId = await DBConn.InsertAsync(new User
                {
                    nickname = param.Nickname,
                    player_id = param.PlayerId,
                    did = param.Did,
                    client_ip = param.ClientIp,
                }, DBTransaction, DefaultTimeout);
                resultCode = lastInsertId > 0 ? ResultCode.Success : ResultCode.DBInsertError;
            }
            else
            {
                isNew = false;
                var isSuccess = await DBConn.UpdateAsync(new User
                {
                     nickname = param.Nickname,
                     player_id = param.PlayerId,
                     did = param.Did,
                     client_ip = param.ClientIp,
                }, DBTransaction, DefaultTimeout);
                resultCode = isSuccess ? ResultCode.Success : ResultCode.DBUpdateError;
            }

            return new Tuple<ResultCode, bool>(resultCode, isNew);
        }

        public async Task<User> GetUserAsync(GetUserParam param)
        {
            if (DBConn == null)
            {
                throw new Exception("DB is not opened");
            }

            // 데이터가 이미 존재하는지 확인
            var iEnumerable = await DBConn.QueryAsync<User>(User.SelectQueryOne, new
            {
                nickname = param.Nickname,
            }, DBTransaction, DefaultTimeout);

            return iEnumerable.First();
        }
    }
}