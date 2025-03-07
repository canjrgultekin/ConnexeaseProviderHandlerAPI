using Serilog;
using ProviderHandlerAPI.Services;
using StackExchange.Redis;
using ProviderHandlerAPI.Services.Ikas;
using ProviderHandlerAPI.Services.Ticimax;
using ProviderHandlerAPI.Services.Tsoft;
using ProviderHandlerAPI.Services.Cache;

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


// 🔥 ProviderHandler DI Entegrasyonu
builder.Services.AddSingleton<ProviderHandler>();

// 🔥 Ikas, Ticimax ve Tsoft API Client DI Entegrasyonu
builder.Services.AddHttpClient<IIkasApiClient, IkasApiClient>();
builder.Services.AddHttpClient<ITicimaxApiClient, TicimaxApiClient>();
builder.Services.AddHttpClient<ITsoftApiClient, TsoftApiClient>();
builder.Services.AddHttpClient();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
