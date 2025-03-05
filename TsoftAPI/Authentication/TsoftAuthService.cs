using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using TsoftAPI.Models;

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

        public async Task<string> GetAuthTokenAsync(string projectName)
        {
            var firmalar = _configuration.GetSection("TsoftAPI").Get<TsoftFirmConfig[]>();
            var firmaConfig = firmalar.FirstOrDefault(f => f.ProjectName == projectName);

            if (firmaConfig == null)
            {
                _logger.LogError($"❌ TsoftAPI için {projectName} konfigürasyonu bulunamadı.");
                throw new Exception($"TsoftAPI için {projectName} yapılandırması bulunamadı.");
            }

            string baseUrl = firmaConfig.BaseUrl;
            string pass = firmaConfig.Password;
            string cacheKey = $"TsoftAuthToken:{projectName}";

            // 🔥 Öncelikle Redis Cache içinde token var mı kontrol edelim
            var cachedToken = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedToken))
            {
                _logger.LogInformation($"✅ {projectName} için token cache'den kullanıldı.");
                return cachedToken;
            }

            var authUrl = $"{baseUrl}/rest1/auth/login/{firmaConfig.Name}?pass={pass}";

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
