using System;
using System.Text.Json;
using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;
using ConnexeaseProviderHandlerAPI.Services;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public class ProviderHandler
    {
        private readonly RedisCacheService _redisCacheService;
        private readonly TicimaxApiClient _ticimaxApiClient;
        private readonly TsoftApiClient _tsoftApiClient; // 🔥 Interface üzerinden bağımlılık yönetiliyor
        private readonly IProviderService _ikasService;

        public ProviderHandler(
            TicimaxApiClient ticimaxApiClient,
            TsoftApiClient tsoftApiClient, // 🔥 Interface olarak eklendi
            IkasService ikasService,
            RedisCacheService redisCacheService)
        {
            _ticimaxApiClient = ticimaxApiClient;
            _tsoftApiClient = tsoftApiClient;
            _ikasService = ikasService;
            _redisCacheService = redisCacheService;
        }

        public async Task<string> HandleRequestAsync(ClientRequestDto request)
        {
            string cacheKey = $"{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";
            var cachedCustomer = await _redisCacheService.GetCacheAsync(cacheKey);

            if ((request.Provider == "Ticimax" || request.Provider == "Tsoft") && cachedCustomer == null)
            {
                Console.WriteLine($"🆕 {request.Provider} müşteri verisi çekiliyor: {request.CustomerId}");

                object customerData = null;

                if (request.Provider == "Ticimax")
                {
                    customerData = await _ticimaxApiClient.GetCustomerData(request.CustomerId);
                }
                else if (request.Provider == "Tsoft")
                {
                    customerData = await _tsoftApiClient.GetCustomerData(request.ProjectName,request.CustomerId);
                }

                if (customerData != null)
                {
                    Console.WriteLine($"✅ {request.Provider} Müşteri verisi cache'e alınıyor: {request.CustomerId}");

                    var newCustomerData = new CustomerData
                    {
                        CustomerId = request.CustomerId,
                        SessionId = request.SessionId,
                        Provider = request.Provider,
                        ProjectName = request.ProjectName,
                        Data = customerData
                    };

                    await _redisCacheService.SetCacheAsync(cacheKey, newCustomerData, 60);
                }
                else
                {
                    Console.WriteLine($"⚠️ {request.Provider} müşteri verisi alınamadı: {request.CustomerId}");
                }
            }

            return request.Provider switch
            {
                "Ticimax" => (await _ticimaxApiClient.SendRequestToTicimaxAsync(request)).Message,
                "Tsoft" => (await _tsoftApiClient.SendRequestToTsoftAsync(request)).Message, // 🔥 TsoftAPI Çağrılıyor
                "Ikas" => await _ikasService.ProcessRequestAsync(request),
                _ => throw new ArgumentException("Geçersiz Provider")
            };
        }
    }
}
