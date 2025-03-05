using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TsoftAPI.Authentication;
using TsoftAPI.Models;

namespace TsoftAPI.Services
{
    public class TsoftService : ITsoftService
    {
        private readonly HttpClient _httpClient;
        private readonly TsoftAuthService _authService;
        private readonly ILogger<TsoftService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        public TsoftService(HttpClient httpClient, IConfiguration configuration, TsoftAuthService authService, ILogger<TsoftService> logger, IDistributedCache cache)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<TsoftResponseDto> HandleTsoftRequestAsync(TsoftRequestDto request)
        {
            // 🔥 Config’den firma bilgilerini çek
            var firmalar = _configuration.GetSection("TsoftAPI").Get<TsoftFirmConfig[]>();
            var firmaConfig = firmalar.FirstOrDefault(f => f.ProjectName == request.ProjectName);

            if (firmaConfig == null)
            {
                _logger.LogError($"❌ TsoftAPI için {request.ProjectName} konfigürasyonu bulunamadı.");
                throw new Exception($"TsoftAPI için {request.ProjectName} yapılandırması bulunamadı.");
            }

            var token = await _authService.GetAuthTokenAsync(request.ProjectName);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            string apiUrl = $"{firmaConfig.BaseUrl}";

            // **Müşteri Verisini Cache’ten Al veya Çek**
            var cacheKey = $"TsoftCustomer:{request.SessionId}:{request.CustomerId}";
            string cachedCustomerCode = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedCustomerCode))
            {
                var customerData = await GetCustomerDataAsync(request.ProjectName, request.CustomerId);
                cachedCustomerCode = customerData?.CustomerCode;

                if (!string.IsNullOrEmpty(cachedCustomerCode))
                {
                    await _cache.SetStringAsync(cacheKey, cachedCustomerCode);
                }
                else
                {
                    _logger.LogWarning($"⚠️ Müşteri kodu bulunamadı: {request.CustomerId}");
                    cachedCustomerCode = "defaultCustomerCode"; // Hata olmaması için varsayılan bir değer atanabilir
                }
            }

            // **ActionType’a Göre Doğru API Endpointini Kullan**
            switch (request.ActionType)
            {
                case "add_to_cart":
                    apiUrl = $"{firmaConfig.BaseUrl}/rest1/customer/getCart/{cachedCustomerCode}";
                    break;

                case "remove_to_cart":
                    apiUrl = $"{firmaConfig.BaseUrl}/rest1/customer/getCart/{cachedCustomerCode}";
                    break;

                case "checkout":
                    apiUrl = $"{firmaConfig.BaseUrl}/rest1/order/get";
                    break;

                case "add_favorite_product":
                    apiUrl = $"{firmaConfig.BaseUrl}/rest1/customer/getWishList";
                    break;

                default:
                    throw new ArgumentException("Geçersiz ActionType");
            }

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TsoftResponseDto>(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Tsoft API çağrısı başarısız: {ex.Message}");
                throw;
            }
        }

        public async Task<TsoftCustomerResponseDto> GetCustomerDataAsync(string projectName, string customerId)
        {
            var token = await _authService.GetAuthTokenAsync(projectName);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var firmalar = _configuration.GetSection("TsoftAPI").Get<TsoftFirmConfig[]>();
            var firmaConfig = firmalar.FirstOrDefault(f => f.ProjectName == projectName);

            if (firmaConfig == null)
            {
                _logger.LogError($"❌ TsoftAPI için {projectName} konfigürasyonu bulunamadı.");
                throw new Exception($"TsoftAPI için {projectName} yapılandırması bulunamadı.");
            }

            string apiUrl = $"{firmaConfig.BaseUrl}/rest1/customer/getCustomerById/{customerId}";

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, new StringContent($"token={token}", Encoding.UTF8, "application/x-www-form-urlencoded"));
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TsoftCustomerResponseDto>(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Tsoft Customer Data alma hatası: {ex.Message}");
                throw;
            }
        }
    }

}
