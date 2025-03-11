using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Redis
{
    public class RedisCacheService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task SetCacheAsync<T>(string key, T value, int expirationMinutes = 30)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, jsonData, TimeSpan.FromMinutes(expirationMinutes));

                _logger.LogInformation($"✅ Redis Cache'e veri eklendi: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Redis Cache'e veri ekleme hatası: {ex.Message}");
            }
        }

        public async Task<string> GetCacheAsync(string key)
        {
            try
            {
                var cachedData = await _db.StringGetAsync(key);
                if (!cachedData.IsNullOrEmpty)
                {
                    _logger.LogInformation($"🔵 Redis Cache'den veri alındı: {key}");
                    return cachedData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Redis Cache'den veri alınamadı: {ex.Message}");
            }

            return null;
        }

        public async Task<T> GetCacheObjectAsync<T>(string key)
        {
            try
            {
                var data = await _db.StringGetAsync(key);
                if (!data.IsNullOrEmpty)
                {
                    _logger.LogInformation($"🔵 Redis Cache'den obje alındı: {key}");
                    return JsonSerializer.Deserialize<T>(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Redis Cache'den obje alınamadı: {ex.Message}");
            }

            return default;
        }

        public async Task RemoveCacheAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                _logger.LogInformation($"❌ Redis Cache'den veri silindi: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Redis Cache'den veri silme hatası: {ex.Message}");
            }
        }
    }
}
