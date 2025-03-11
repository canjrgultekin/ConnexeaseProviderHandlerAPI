using System;
using System.Text.Json;
using System.Threading.Tasks;
using ProviderHandlerAPI.Models;
using ProviderHandlerAPI.Services;
using ProviderHandlerAPI.Enums;
using ProviderHandlerAPI.Services.Ticimax;
using ProviderHandlerAPI.Services.Tsoft;
using ProviderHandlerAPI.Helper;
using ProviderHandlerAPI.Services.Ikas;
using Common.Kafka;
using Common.Redis;

namespace ProviderHandlerAPI.Services
{
    public class ProviderHandler
    {
        private readonly RedisCacheService _redisCacheService;
        private readonly ITicimaxApiClient _ticimaxApiClient;
        private readonly ITsoftApiClient _tsoftApiClient;
        private readonly IIkasApiClient _ikasApiClient;
        private readonly KafkaProducerService _kafkaProducer; // 🔥 Kafka Producer eklendi

        public ProviderHandler(
            ITicimaxApiClient ticimaxApiClient,
            ITsoftApiClient tsoftApiClient,
            IIkasApiClient ikasApiClient,
            RedisCacheService redisCacheService,
            KafkaProducerService kafkaProducer)
        {
            _ticimaxApiClient = ticimaxApiClient;
            _tsoftApiClient = tsoftApiClient;
            _ikasApiClient = ikasApiClient;
            _redisCacheService = redisCacheService;
            _kafkaProducer = kafkaProducer; // 🔥 Kafka Producer kullanımı

        }

        public async Task<object> HandleRequestAsync(ClientRequestDto request)
        {

            if (!Enum.TryParse(request.Provider, true, out ProviderType providerType))
            {
                throw new ArgumentException("Geçersiz Provider");
            }


            object customerData = providerType.GetProviderTypeString() switch
            {
                "ticimax" => await _ticimaxApiClient.GetCustomerDataAsync(request),
                "tsoft" => await _tsoftApiClient.GetCustomerDataAsync(request),
                "ikas" => await _ikasApiClient.GetCustomerDataAsync(request),

                _ => null
            };

            if (customerData == null)
            {
                Console.WriteLine($"⚠️ {providerType} müşteri verisi alınamadı: {request.CustomerId}");
            }

            object data = providerType.GetProviderTypeString() switch
            {
                "ticimax" => await _ticimaxApiClient.SendRequestToTicimaxAsync(request),
                "tsoft" => await _tsoftApiClient.SendRequestToTsoftAsync(request),
                "ikas" => await _ikasApiClient.SendRequestToIkasAsync(request),
                _ => null
            };
            if (data == null && customerData == null)
            {
                Console.WriteLine($"⚠️ {providerType} servis verisi alınamadı: {request.CustomerId}");

                return new ClientResponseDto();
            }
            else
            {
                var responseDto = new ClientResponseDto
                {
                    CustomerId = request.CustomerId,
                    SessionId = request.SessionId,
                    Provider = request.Provider,
                    ProjectName = request.ProjectName,
                    CustomerDataById = customerData,
                    ServiceDataByActionType = data
                };
                // 🔥 Kafka'ya event gönderiliyor
                await _kafkaProducer.SendMessageAsync(new
                {
                    Provider = request.Provider,
                    ProjectName = request.ProjectName,
                    SessionId = request.SessionId,
                    CustomerId = request.CustomerId,
                    ActionType = request.ActionType,
                    Data = responseDto
                });
                return responseDto;
            }  
        }
    }
}
