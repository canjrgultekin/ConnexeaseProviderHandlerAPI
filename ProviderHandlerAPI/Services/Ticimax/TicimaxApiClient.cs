using System.Text;
using System.Text.Json;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Models.Ticimax;

namespace ProviderHandlerAPI.Services.Ticimax
{
    public class TicimaxApiClient : ITicimaxApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _ticimaxApiUrl;
        private readonly ILogger<TicimaxApiClient> _logger;

        public TicimaxApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TicimaxApiClient> logger)
        {
            _httpClient = httpClient;
            _ticimaxApiUrl = configuration["TicimaxAPI:BaseUrl"];
            _logger = logger;
        }

        public async Task<object> GetCustomerDataAsync(ClientRequestDto request)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_ticimaxApiUrl}/api/ticimax/get-customer?projectName={request.ProjectName} &customerId= {request.CustomerId}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tsoftCustomerResponse = JsonSerializer.Deserialize<object>(jsonResponse);

                return tsoftCustomerResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TicimaxAPI'den müşteri bilgisi alınamadı: {ex.Message}");
                return new { Message = "Müşteri bilgisi alınamadı" };
            }
        }

        public async Task<TicimaxResponseDto> SendRequestToTicimaxAsync(ClientRequestDto request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_ticimaxApiUrl}/api/ticimax/process", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<object>(jsonResponse);
                TicimaxResponseDto responseDto = new TicimaxResponseDto
                {
                    Status = "Success",
                    Message = $"{request.ProjectName} için Tsoft işlemi tamamlandı",
                    Data = data
                };
                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ TicimaxAPI çağrısı başarısız: {ex.Message}");
                return new TicimaxResponseDto { Status = "Error", Message = "Ticimax API çağrısı başarısız" };
            }
        }
    }
}
