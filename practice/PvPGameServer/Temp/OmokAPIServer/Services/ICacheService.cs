using System.Threading.Tasks;

namespace OmokAPIServer.Services
{
    public interface ICacheService
    {
        public Task<TObj> GetAsync<TObj>(string key);

        public Task<bool> SetAsync<TObj>(string key, TObj obj, int expireSec);

        public Task<bool> AddListAsync(string key, byte[] bytes);

        public Task<bool> DeleteListAsync(string key);
    }
}