using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProviderHandlerAPI.Models;

namespace ProviderHandlerAPI.Services.Ikas
{
    public class IkasApiClient : IIkasApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _ikasApiUrl;
        private readonly ILogger<IkasApiClient> _logger;
        private readonly RedisCacheService _cacheService;

        public IkasApiClient(HttpClient httpClient, IConfiguration configuration,RedisCacheService redisCacheService, ILogger<IkasApiClient> logger)
        {
            _httpClient = httpClient;
            _ikasApiUrl = configuration["IkasAPI:BaseUrl"]; // 🔥 BaseUrl artık config'den geliyor
            _logger = logger;
            _cacheService = redisCacheService;
        }

        public async Task<object> GetCustomerDataAsync(ClientRequestDto request)
        {
            string cacheKey = $"IkasCustomerData:{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";

            // 🟢 Cache kontrolü
            var cachedData = await _cacheService.GetCacheObjectAsync<object>(cacheKey);
            if (cachedData != null) return cachedData;
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_ikasApiUrl}/api/ikas/get-customer", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var customerData = JsonSerializer.Deserialize<object>(jsonResponse);

                // 🔵 Cache'e ekleyelim
                await _cacheService.SetCacheAsync(cacheKey, customerData, 10);
                return customerData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ IkasAPI'den müşteri bilgisi alınamadı: {ex.Message}");
                return new { Message = "Müşteri bilgisi alınamadı" };
            }
        }

        public async Task<object> SendRequestToIkasAsync(ClientRequestDto request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_ikasApiUrl}/api/ikas/process", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ IkasAPI çağrısı başarısız: {ex.Message}");
                return new { Status = "Error", Message = "Ikas API çağrısı başarısız" };
            }
        }
    }
}
