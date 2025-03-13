using System;
using System.Text;
using System.Security.Cryptography;
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
        private readonly KafkaProducerService _kafkaProducer;

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
            _kafkaProducer = kafkaProducer;
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

                // 🔥 Tüm verinin hash'ini al (tamamı mükerrer mi?)
                string fullMessageHash = GenerateHash(JsonSerializer.Serialize(responseDto));
                string fullRedisKey = $"kafka_event:{fullMessageHash}";

                // 🔥 Sadece CustomerDataById'nin hash'ini al (müşteri verisi değişti mi?)
                string customerDataHash = customerData != null ? GenerateHash(JsonSerializer.Serialize(customerData)) : null;
                string customerRedisKey = $"customer_data:{request.CustomerId}";

                // 🔥 Redis Duplicate Kontrolü
                bool isFullDuplicate = await _redisCacheService.GetCacheAsync(fullRedisKey) != null;
                bool isCustomerDuplicate = customerDataHash != null && await _redisCacheService.GetCacheAsync(customerRedisKey) == customerDataHash;

                // 🔥 Eğer tamamen mükerrer ise, Kafka'ya göndermeden çık
                if (isFullDuplicate)
                {
                    Console.WriteLine($"⚠️ Tamamı mükerrer olan Kafka event tespit edildi: {fullMessageHash}");
                    return responseDto;
                }

                // 🔥 Eğer sadece CustomerDataById mükerrer ise, `null` olarak Kafka'ya gönder
                if (isCustomerDuplicate)
                {
                    responseDto.CustomerDataById = null;
                    Console.WriteLine($"⚠️ CustomerDataById değişmedi, null olarak gönderilecek.");
                }

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

                // 🔥 Redis'e ekle ve TTL belirle (örn: 1 saat)
                await _redisCacheService.SetCacheAsync(fullRedisKey, "1", 60);

                // 🔥 Eğer CustomerDataById değiştiyse, yeni hash değerini kaydet
                if (customerDataHash != null)
                {
                    await _redisCacheService.SetCacheAsync(customerRedisKey, customerDataHash, 60);
                }

                return responseDto;
            }
        }

        /// <summary>
        /// Verilen string verisini SHA256 hash'ine çevirir
        /// </summary>
        private string GenerateHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
