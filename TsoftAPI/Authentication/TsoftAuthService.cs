using System.Text.Json;
using Common.Redis;
using Microsoft.Extensions.Caching.Distributed;
using TsoftAPI.Helper;
using TsoftAPI.Models;
using TsoftAPI.Models.Authentication;

namespace TsoftAPI.Authentication
{
    public class TsoftAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TsoftAuthService> _logger;
        private readonly RedisCacheService _cache;

        public TsoftAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<TsoftAuthService> logger, RedisCacheService cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        public async Task<string> GetAuthTokenAsync(TsoftRequestDto request)
        {
            string cacheKey = $"TsoftCustomerToken:{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";
            var cachedToken = await _cache.GetCacheObjectAsync<string>(cacheKey);
            if (cachedToken != null) return cachedToken;

            var firmaConfig = Utils.GetFirmaConfig(_configuration, _logger, request.ProjectName); // 🔥 Helper Metot Kullanılıyo

            var authUrl = $"{firmaConfig.BaseUrl}/rest1/auth/login/{firmaConfig.Name}?pass={firmaConfig.Password}";

            try
            {
                _logger.LogInformation($"📡 {request.ProjectName} için Tsoft API'ye token isteği gönderiliyor: {authUrl}");

                var response = await _httpClient.PostAsync(authUrl, null);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<TsoftAuthResponseDto>(jsonResponse);

                var token = authResponse?.Data?[0]?.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    await _cache.SetCacheAsync<string>(cacheKey, token,10);
                    return token;
                }
                throw new Exception("Tsoft API Token alınamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ {request.ProjectName} için Tsoft API'ye token isteği başarısız: {ex.Message}");
                throw;
            }
        }
    }
}
