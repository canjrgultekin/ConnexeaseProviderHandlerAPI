using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Kafka
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
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = configuration["Kafka:SaslUsername"],
                SaslPassword = configuration["Kafka:SaslPassword"],
                ClientId = configuration["Kafka:ClientId"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _topic = configuration["Kafka:Topic"];
            _logger = logger;
        }

        public async Task SendMessageAsync(object message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            try
            {
                await _producer.ProduceAsync(_topic, new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = jsonMessage });
                _logger.LogInformation($"📡 Kafka'ya mesaj gönderildi: {jsonMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Kafka'ya mesaj gönderilirken hata oluştu: {ex.Message}");
            }
        }
    }
}
