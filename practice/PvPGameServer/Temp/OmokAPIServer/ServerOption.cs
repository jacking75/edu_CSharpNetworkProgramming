using OmokAPIServer.Models;

namespace OmokAPIServer.Options
{
    public class ServerOption
    {
        // DB
        public string DBConnStr { get; set; }
        public int DBTimeout { get; set; }
        
        // 캐시
        public string CacheConnStr { get; set; }
        public double SessionExpireTime { get; set; }
        
        // 세션
        public int AuthTokenLength { get; set; }
        
        // 게임 서버 정보
        public PvPGameServerGroup GameServerGroup { get; set; }
    }
}