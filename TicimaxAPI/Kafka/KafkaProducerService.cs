using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TicimaxAPI.Kafka
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _topic = configuration["Kafka:Topic"];
            _logger = logger;
        }

        public async Task PublishMessageAsync(string key, string message)
        {
            try
            {
                var kafkaMessage = new Message<string, string> { Key = key, Value = message };
                await _producer.ProduceAsync(_topic, kafkaMessage);

                _logger.LogInformation($"📤 Kafka'ya mesaj gönderildi: {message}");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"❌ Kafka mesaj gönderme hatası: {ex.Message}");
            }
        }
    }
}
