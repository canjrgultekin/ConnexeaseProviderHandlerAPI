using System.Text;
using System.Text.Json;
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

        public TsoftApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TsoftApiClient> logger)
        {
            _httpClient = httpClient;
            _tsoftApiUrl = configuration["TsoftAPI:BaseUrl"]; // 🔥 BaseUrl artık config'den geliyor
           
            _logger = logger;
        }

        public async Task<TsoftCustomerResponseDto> GetCustomerDataAsync(ClientRequestDto request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_tsoftApiUrl}/api/tsoft/get-customer",content);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tsoftCustomerResponse = JsonSerializer.Deserialize<TsoftCustomerResponseDto>(jsonResponse);
                return tsoftCustomerResponse;
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
