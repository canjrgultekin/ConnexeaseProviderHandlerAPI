using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ConnexeaseProviderHandlerAPI.Services.Cache
{
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisCacheService(IConfiguration configuration)
        {
            _redis = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
            _db = _redis.GetDatabase();
        }

        public async Task SetCacheAsync(string key, object value, int expirationMinutes = 30)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
            };
            var jsonData = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, jsonData, TimeSpan.FromMinutes(expirationMinutes));
        }

        public async Task<string> GetCacheAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }

        public async Task<T> GetCacheObjectAsync<T>(string key)
        {
            var data = await _db.StringGetAsync(key);
            return data.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task RemoveCacheAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}
