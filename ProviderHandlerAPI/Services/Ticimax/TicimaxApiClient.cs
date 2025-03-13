using System.Text;
using System.Text.Json;
using Common.Redis;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Ticimax;

namespace ProviderHandlerAPI.Services.Ticimax
{
    public class TicimaxApiClient : ITicimaxApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _ticimaxApiUrl;
        private readonly ILogger<TicimaxApiClient> _logger;
        private readonly RedisCacheService _cacheService;

        public TicimaxApiClient(HttpClient httpClient, IConfiguration configuration,RedisCacheService redisCache, ILogger<TicimaxApiClient> logger)
        {
            _httpClient = httpClient;
            _ticimaxApiUrl = configuration["TicimaxAPI:BaseUrl"];
            _logger = logger;
            _cacheService = redisCache;
        }

        public async Task<object> GetCustomerDataAsync(ClientRequestDto request)
        {
            string cacheKey = $"TicimaxCustomerData:{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";

            // 🟢 Önce Cache'den kontrol edelim
            var cachedData = await _cacheService.GetCacheObjectAsync<object>(cacheKey);
            if (cachedData != null) return cachedData;
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_ticimaxApiUrl}/api/ticimax/get-customer",content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var customerData = JsonSerializer.Deserialize<object>(jsonResponse);

                // 🔵 Cache'e ekleyelim
                await _cacheService.SetCacheAsync(cacheKey, customerData, 10);
                return customerData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TicimaxAPI'den müşteri bilgisi alınamadı: {ex.Message}");
                return new { Message = "Müşteri bilgisi alınamadı" };
            }
        }

        public async Task<object> SendRequestToTicimaxAsync(ClientRequestDto request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_ticimaxApiUrl}/api/ticimax/process", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<object>(jsonResponse);
               
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TicimaxAPI çağrısı başarısız: {ex.Message}");
                return new  { Message = "Ticimax API çağrısı başarısız" };
            }
        }
    }
}
