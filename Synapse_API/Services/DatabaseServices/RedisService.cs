using StackExchange.Redis;

namespace Synapse_API.Services.DatabaseServices
{
    public class RedisService
    {
        private readonly IDatabase _redisDb;
        public RedisService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }
        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _redisDb.StringSetAsync(key, value, expiry);
        }
        public async Task<string> GetAsync(string key)
        {
            var value = await _redisDb.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return null;
            }
            return value.ToString();
        }
        public async Task<bool> DeleteAsync(string key)
        {
            return await _redisDb.KeyDeleteAsync(key);
        }
        public async Task<bool> ExistsAsync(string key)
        {
            return await _redisDb.KeyExistsAsync(key);
        }
        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return await _redisDb.KeyExpireAsync(key, expiry);
        }
    }
}
