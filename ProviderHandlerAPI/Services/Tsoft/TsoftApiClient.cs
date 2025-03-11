using System.Text;
using System.Text.Json;
using Common.Redis;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Tsoft;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProviderHandlerAPI.Services.Tsoft
{
    public class TsoftApiClient : ITsoftApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _tsoftApiUrl;
        private readonly ILogger<TsoftApiClient> _logger;
        private readonly RedisCacheService _cacheService;

        public TsoftApiClient(HttpClient httpClient, IConfiguration configuration,RedisCacheService redisCache, ILogger<TsoftApiClient> logger)
        {
            _httpClient = httpClient;
            _tsoftApiUrl = configuration["TsoftAPI:BaseUrl"]; // 🔥 BaseUrl artık config'den geliyor
            _logger = logger;
            _cacheService = redisCache;
        }

        public async Task<TsoftCustomerResponseDto> GetCustomerDataAsync(ClientRequestDto request)
        {
            string cacheKey = $"{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";

            // 🟢 Cache kontrolü
            var cachedData = await _cacheService.GetCacheObjectAsync<TsoftCustomerResponseDto>(cacheKey);
            if (cachedData != null) return cachedData;
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_tsoftApiUrl}/api/tsoft/get-customer",content);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var customerData = JsonSerializer.Deserialize<TsoftCustomerResponseDto>(jsonResponse);

                // 🔵 Cache'e ekleyelim
                await _cacheService.SetCacheAsync(cacheKey, customerData, 10);
                return customerData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TsoftAPI'den müşteri bilgisi alınamadı: {ex.Message}");
                return new TsoftCustomerResponseDto();
            }
        }

        public async Task<TsoftResponseDto> SendRequestToTsoftAsync(ClientRequestDto request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_tsoftApiUrl}/api/tsoft/process", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<object>(jsonResponse);
                TsoftResponseDto responseDto = new TsoftResponseDto
                {
                    Status = "Success",
                    Message = $"{request.ProjectName} için Tsoft işlemi tamamlandı",
                    Data = data
                };
                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TsoftAPI çağrısı başarısız: {ex.Message}");
                return new TsoftResponseDto { Status = "Error", Message = "Tsoft API çağrısı başarısız" };
            }
        }
    }
}
