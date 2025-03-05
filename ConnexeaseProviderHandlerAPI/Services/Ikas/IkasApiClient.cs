using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services.Ikas
{
    public class IkasApiClient : IIkasApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _ikasApiUrl;
        private readonly ILogger<IkasApiClient> _logger;

        public IkasApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<IkasApiClient> logger)
        {
            _httpClient = httpClient;
            _ikasApiUrl = configuration["IkasAPI:BaseUrl"]; // 🔥 BaseUrl artık config'den geliyor
            _logger = logger;
        }

        public async Task<object> GetCustomerDataAsync(ClientRequestDto request)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_ikasApiUrl}/api/ikas/get-customer?projectName={request.ProjectName}&customerId={request.CustomerId}&authToken={request.AuthToken}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(jsonResponse);
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
