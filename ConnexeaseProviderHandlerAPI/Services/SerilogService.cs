using System;
using Serilog;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public static class SerilogService
    {
        public static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void LogInfo(string message)
        {
            Log.Information(message);
        }

        public static void LogError(string message)
        {
            Log.Error(message);
        }
    }
}
