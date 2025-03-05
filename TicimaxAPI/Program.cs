using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicimaxAPI.Services;
using TicimaxAPI.Repositories;
using TicimaxAPI.Kafka;
using Serilog;
using System.ServiceModel;
using TicimaxAPI.WcfServices;

var builder = WebApplication.CreateBuilder(args);

// Serilog Loglama
builder.Host.UseSerilog((context, config) => config.WriteTo.Console());

builder.Services.AddControllers();
//builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new TicimaxWcfClient(
        config["TicimaxAPI:UyeKodu"],
        config["TicimaxAPI:SiteName"],
        config["TicimaxAPI:BaseUrl"]
    );
});

//builder.Services.AddSingleton<ITicimaxRepository, TicimaxRepository>();
builder.Services.AddSingleton<ITicimaxService, TicimaxService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
