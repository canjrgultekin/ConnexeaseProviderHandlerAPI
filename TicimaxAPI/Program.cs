using TicimaxAPI.Services;
using Serilog;
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

// 🔥 TicimaxWcfClient DI Entegrasyonu
builder.Services.AddSingleton<TicimaxWcfClient>();

// 🔥 Ticimax Servisleri DI Entegrasyonu
builder.Services.AddSingleton<ITicimaxService, TicimaxService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
