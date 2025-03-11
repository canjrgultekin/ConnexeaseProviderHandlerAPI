using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KafkaConsumerWorkerService.Services;
using KafkaConsumerWorkerService;

namespace KafkaConsumerWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // appsettings.json yükleniyor
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<KafkaConsumerService>(); // Consumer servisimiz
                    services.AddHostedService<Worker>(); // Worker
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Konsola loglama
                    logging.AddConsole();
                });
    }
}
