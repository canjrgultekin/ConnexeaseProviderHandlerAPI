using Serilog;
using ProviderHandlerAPI.Services;
using StackExchange.Redis;
using ProviderHandlerAPI.Services.Ikas;
using ProviderHandlerAPI.Services.Ticimax;
using ProviderHandlerAPI.Services.Tsoft;
using Common.Kafka;
using Polly.Extensions.Http;
using Polly;
using Common.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog Konfigürasyonu
SerilogService.ConfigureLogging();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔥 Redis Cache Kullanımı
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"])
);
builder.Services.AddSingleton<RedisCacheService>();

// 🔥 HttpClient Timeout, Retry ve Circuit Breaker Politikaları
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10); // 10 saniye timeout

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

// 🔥 ProviderHandler DI Entegrasyonu
builder.Services.AddSingleton<ProviderHandler>();

// 🔥 Ikas, Ticimax ve Tsoft API Client DI Entegrasyonu
builder.Services.AddHttpClient<IIkasApiClient, IkasApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(retryPolicy)
  .AddPolicyHandler(circuitBreakerPolicy)
  .AddPolicyHandler(timeoutPolicy);

builder.Services.AddHttpClient<ITicimaxApiClient, TicimaxApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(retryPolicy)
  .AddPolicyHandler(circuitBreakerPolicy)
  .AddPolicyHandler(timeoutPolicy);

builder.Services.AddHttpClient<ITsoftApiClient, TsoftApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(retryPolicy)
  .AddPolicyHandler(circuitBreakerPolicy)
  .AddPolicyHandler(timeoutPolicy);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<KafkaProducerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
