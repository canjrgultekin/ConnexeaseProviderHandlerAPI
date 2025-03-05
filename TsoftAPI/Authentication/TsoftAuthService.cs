using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TsoftAPI.Helper;
using TsoftAPI.Models.Authentication;

namespace TsoftAPI.Authentication
{
    public class TsoftAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TsoftAuthService> _logger;
        private readonly IDistributedCache _cache;

        public TsoftAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<TsoftAuthService> logger, IDistributedCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        public async Task<string> GetAuthTokenAsync(string projectName,string sessionId)
        {
            var firmaConfig = Utils.GetFirmaConfig(_configuration, _logger, projectName); // 🔥 Helper Metot Kullanılıyor
            string cacheKey = $"TsoftAuthToken:{projectName+sessionId}";
            var cachedToken = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedToken))
            {
                _logger.LogInformation($"✅ {projectName} için token cache'den kullanıldı.");
                return cachedToken;
            }

            var authUrl = $"{firmaConfig.BaseUrl}/rest1/auth/login/{firmaConfig.Name}?pass={firmaConfig.Password}";

            try
            {
                _logger.LogInformation($"📡 {projectName} için Tsoft API'ye token isteği gönderiliyor: {authUrl}");

                var response = await _httpClient.PostAsync(authUrl, null);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<TsoftAuthResponseDto>(jsonResponse);

                var token = authResponse?.Data?[0]?.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    await _cache.SetStringAsync(cacheKey, token, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    });
                    return token;
                }
                throw new Exception("Tsoft API Token alınamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ {projectName} için Tsoft API'ye token isteği başarısız: {ex.Message}");
                throw;
            }
        }
    }
}
