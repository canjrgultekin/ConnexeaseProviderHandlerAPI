using KafkaConsumerWorkerService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaConsumerWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly KafkaConsumerService _consumerService;

        public Worker(ILogger<Worker> logger, KafkaConsumerService consumerService)
        {
            _logger = logger;
            _consumerService = consumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker service başlatılıyor...");

            // Consumer döngüsünü ayrı bir Task olarak çalıştırabiliriz
            await Task.Run(() => _consumerService.StartListeningAsync(stoppingToken), stoppingToken);

            // Bu noktadan sonra bu Task, cancellationToken ile durdurulana kadar çalışmaya devam eder.
        }
    }
}
