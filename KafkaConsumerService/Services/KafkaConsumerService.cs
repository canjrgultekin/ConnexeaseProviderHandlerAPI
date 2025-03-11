using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaConsumerWorkerService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace KafkaConsumerWorkerService.Services
{
    public class KafkaConsumerService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<string, string> _consumer;

        public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("PostgreSQL");

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _configuration["Kafka:SaslUsername"],
                SaslPassword = _configuration["Kafka:SaslPassword"],
                GroupId = _configuration["Kafka:ConsumerGroup"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(_configuration["Kafka:Topic"]);
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🎧 Kafka Consumer başlatıldı...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var eventData = JsonSerializer.Deserialize<EventData>(consumeResult.Value);

                    await SaveToDatabase(eventData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Kafka mesajı işlenirken hata oluştu: {ex.Message}");
                }
            }
        }

        private async Task SaveToDatabase(EventData eventData)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("INSERT INTO kafka_events (provider, project_name, session_id, customer_id, action_type, data) VALUES (@provider, @project_name, @session_id, @customer_id, @action_type, @data)", conn);
            cmd.Parameters.AddWithValue("@provider", eventData.Provider);
            cmd.Parameters.AddWithValue("@project_name", eventData.ProjectName);
            cmd.Parameters.AddWithValue("@session_id", eventData.SessionId);
            cmd.Parameters.AddWithValue("@customer_id", eventData.CustomerId);
            cmd.Parameters.AddWithValue("@action_type", eventData.ActionType);
            cmd.Parameters.AddWithValue("@data", NpgsqlTypes.NpgsqlDbType.Jsonb,eventData.Data);

            await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation($"✅ Kafka event PostgreSQL'e kaydedildi: {JsonSerializer.Serialize(eventData)}");
        }
    }
}
