using TicimaxAPI.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Loglama
builder.Host.UseSerilog((context, config) => config.WriteTo.Console());

builder.Services.AddControllers();

// 🔥 Redis Cache Kullanımı
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "TicimaxCache_";
});

// 🔥 TicimaxWcfClient DI Entegrasyonu
builder.Services.AddSingleton<TicimaxWcfClient>();

// 🔥 Ticimax Servisleri DI Entegrasyonu
builder.Services.AddSingleton<ITicimaxService, TicimaxService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
