using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using IkasAPI.Services;
using StackExchange.Redis;
using Serilog;
using Polly.Extensions.Http;
using Polly;
using Common.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog Loglama
builder.Host.UseSerilog((context, config) => config.WriteTo.Console());

builder.Services.AddControllers();

// 🔥 Redis Bağlantısı (IConnectionMultiplexer ile bağlantı havuzu yönetimi)
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"])
);

// 🔥 RedisCacheService'in DI ile Yönetilmesi
builder.Services.AddSingleton<RedisCacheService>();

// 🔥 Ikas API Service Entegrasyonu
// 🔥 Polly Politikaları
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

// 🔥 HttpClientFactory Kullanımı
builder.Services.AddHttpClient<IIkasService, IkasService>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy)
    .AddPolicyHandler(timeoutPolicy);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
