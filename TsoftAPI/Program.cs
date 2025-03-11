using TsoftAPI.Authentication;
using TsoftAPI.Services;
using Serilog;
using Polly.Extensions.Http;
using Polly;
using Common.Redis;
using StackExchange.Redis;

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
// 🔥 Polly Politikaları
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

// 🔥 HttpClientFactory Kullanımı
builder.Services.AddHttpClient<ITsoftService, TsoftService>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy)
    .AddPolicyHandler(timeoutPolicy);

builder.Services.AddHttpClient<TsoftAuthService>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy)
    .AddPolicyHandler(timeoutPolicy);


var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
