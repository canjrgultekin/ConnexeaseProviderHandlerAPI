using TsoftAPI.Authentication;
using TsoftAPI.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Loglama
builder.Host.UseSerilog((context, config) => config.WriteTo.Console());

builder.Services.AddControllers();

// 🔥 Redis Kullanımı İçin:
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "TsoftCache_";
});

builder.Services.AddHttpClient<TsoftAuthService>();
builder.Services.AddHttpClient<ITsoftService, TsoftService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
