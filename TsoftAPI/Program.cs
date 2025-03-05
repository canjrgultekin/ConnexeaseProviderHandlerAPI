using Microsoft.Extensions.Caching.Distributed;
using TsoftAPI.Authentication;
using TsoftAPI.Services;
using StackExchange.Redis;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// CORS EKLENDİ 🔥
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Serilog Loglama
builder.Host.UseSerilog((context, config) => config.WriteTo.Console());

builder.Services.AddControllers();

// 🔥 Redis Kullanımı İçin:
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "TsoftCache_";
});
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpClient<TsoftAuthService>();
builder.Services.AddHttpClient<ITsoftService, TsoftService>();

var app = builder.Build();

app.UseCors("AllowAll"); // 🔥 CORS EKLENDİ
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
