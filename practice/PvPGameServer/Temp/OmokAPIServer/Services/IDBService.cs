using System;
using System.Threading.Tasks;
using OmokAPIServer.Enums;
using OmokAPIServer.Models;
using OmokAPIServer.Models.Param;
using OmokAPIServer.Models.Table;

namespace OmokAPIServer.Services
{
    public interface IDBService
    {
        // DB 열기.
        public void Open();
        
        // DB 닫기.
        public void Close();
        
        // 트랜잭션 시작.
        public void StartTransaction();
        
        // 트랜잭션 롤백.
        public void Rollback();
        
        // 트랜잭션 커밋.
        public void Commit();

        // 유저 데이터 업데이트
        public Task<Tuple<ResultCode, bool>> UpdateUserAsync(UpdateUserParam param);
        
        // 유저 데이터 가져오기
        public Task<User> GetUserAsync(GetUserParam param);
    }
}