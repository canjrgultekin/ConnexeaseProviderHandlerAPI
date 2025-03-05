using System;
using System.Text.Json;
using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;
using ConnexeaseProviderHandlerAPI.Services;
using ConnexeaseProviderHandlerAPI.Enums;
using ConnexeaseProviderHandlerAPI.Services.Ticimax;
using ConnexeaseProviderHandlerAPI.Services.Tsoft;
using ConnexeaseProviderHandlerAPI.Services.Cache;
using ConnexeaseProviderHandlerAPI.Helper;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public class ProviderHandler
    {
        private readonly RedisCacheService _redisCacheService;
        private readonly ITicimaxApiClient _ticimaxApiClient;
        private readonly ITsoftApiClient _tsoftApiClient;
        private readonly IProviderService _ikasService;

        public ProviderHandler(
            ITicimaxApiClient ticimaxApiClient,
            ITsoftApiClient tsoftApiClient,
            IProviderService ikasService,
            RedisCacheService redisCacheService)
        {
            _ticimaxApiClient = ticimaxApiClient;
            _tsoftApiClient = tsoftApiClient;
            _ikasService = ikasService;
            _redisCacheService = redisCacheService;
        }

        public async Task<object> HandleRequestAsync(ClientRequestDto request)
        {
            string cacheKey = $"{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";
            var cachedCustomer = await _redisCacheService.GetCacheAsync(cacheKey);

            if (!Enum.TryParse(request.Provider, true, out ProviderType providerType))
            {
                throw new ArgumentException("Geçersiz Provider");
            }

            if (cachedCustomer == null)
            {
                Console.WriteLine($"🆕 {providerType} müşteri verisi çekiliyor: {request.CustomerId}");

                object customerData = providerType.GetProviderTypeString() switch
                {
                    "ticimax" => await _ticimaxApiClient.GetCustomerDataAsync(request),
                    "tsoft" => await _tsoftApiClient.GetCustomerDataAsync(request),
                    _ => null
                };

                if (customerData != null)
                {
                    Console.WriteLine($"✅ {providerType} Müşteri verisi cache'e alınıyor: {request.CustomerId}");

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
                    Console.WriteLine($"⚠️ {providerType} müşteri verisi alınamadı: {request.CustomerId}");
                }
            }
            object data = providerType.GetProviderTypeString() switch
            {
                "ticimax" => await _ticimaxApiClient.SendRequestToTicimaxAsync(request),
                "tsoft" => await _tsoftApiClient.SendRequestToTsoftAsync(request),
                "ikas" => await _ikasService.ProcessRequestAsync(request),
                _ => throw new ArgumentException("Geçersiz Provider")
            };
            return data;
        }
    }
}
