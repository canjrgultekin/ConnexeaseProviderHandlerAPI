using System.Text;
using System.Text.Json;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public class TsoftApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _tsoftApiUrl;
        private readonly ILogger<TsoftApiClient> _logger;

        public TsoftApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TsoftApiClient> logger)
        {
            _httpClient = httpClient;
            _tsoftApiUrl = configuration["TsoftAPI:BaseUrl"]; // 🔥 BaseUrl artık config'den geliyor
            _logger = logger;
        }

        public async Task<object> GetCustomerData(string projectName,string customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_tsoftApiUrl}/api/tsoft/get-customer?projectName={projectName}&customerId={customerId}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TsoftAPI'den müşteri bilgisi alınamadı: {ex.Message}");
                return new { Message = "Müşteri bilgisi alınamadı" };
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
                return JsonSerializer.Deserialize<TsoftResponseDto>(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TsoftAPI çağrısı başarısız: {ex.Message}");
                return new TsoftResponseDto { Status = "Error", Message = "Tsoft API çağrısı başarısız" };
            }
        }
    }
}
