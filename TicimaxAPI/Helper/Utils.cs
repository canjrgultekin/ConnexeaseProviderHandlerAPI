using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicimaxAPI.Models;

namespace TicimaxAPI.Helper
{
    public static class Utils
    {
        public static TicimaxFirmConfig GetFirmaConfig(IConfiguration configuration, ILogger logger, string projectName)
        {
            var firmalar = configuration.GetSection("TicimaxAPI").Get<TicimaxFirmConfig[]>();
            var firmaConfig = firmalar.FirstOrDefault(f => f.ProjectName.ToLower() == projectName.ToLower());

            if (firmaConfig == null)
            {
                logger.LogError($"❌ TicimaxAPI için {projectName} konfigürasyonu bulunamadı.");
                throw new Exception($"TicimaxAPI için {projectName} yapılandırması bulunamadı.");
            }

            return firmaConfig;
        }
    }
}
