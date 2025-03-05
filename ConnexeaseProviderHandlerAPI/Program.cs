using Serilog;
using ConnexeaseProviderHandlerAPI.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog Konfigürasyonu
SerilogService.ConfigureLogging();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"])
);
builder.Services.AddSingleton<RedisCacheService>();

builder.Services.AddSingleton<IkasService>();
builder.Services.AddSingleton<ProviderHandler>();
//builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHttpClient<TicimaxApiClient>(); // HTTP Client kullanarak API çağrısı yapacak
builder.Services.AddHttpClient<TsoftApiClient>(); // 🔥 Interface olarak ekleniyor


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
