using System;
using System.Threading.Tasks;
using TicimaxAPI.Kafka;
using TicimaxAPI.Models;
using TicimaxAPI.Repositories;

namespace TicimaxAPI.Services
{
    public class TicimaxService : ITicimaxService
    {
        private readonly TicimaxWcfClient _ticimaxWcfClient;
    //    private readonly KafkaProducerService _kafkaProducerService;

        public TicimaxService(TicimaxWcfClient ticimaxWcfClient)//, KafkaProducerService kafkaProducerService)
        {
            _ticimaxWcfClient = ticimaxWcfClient;
   //         _kafkaProducerService = kafkaProducerService;
        }

        public async Task<TicimaxResponseDto> HandleTicimaxRequestAsync(TicimaxRequestDto request)
        {
            var data = await _ticimaxWcfClient.HandleActionAsync(request);

            // Kafka'ya mesaj gönder
       //     await _kafkaProducerService.PublishMessageAsync(request.SessionId, data.ToString());

            return new TicimaxResponseDto
            {
                Status = "Success",
                Message = "Ticimax işlemi tamamlandı",
                Data = data
            };
        }
        public async Task<object> GetCustomerDataAsync(string customerId)
        {
            return await _ticimaxWcfClient.GetCustomerData(customerId);
        }
    }
}
