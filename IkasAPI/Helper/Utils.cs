using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IkasAPI.Models;

namespace IkasAPI.Helper
{
    public static class Utils
    {
        public static IkasFirmConfig GetIkasConfig(IConfiguration configuration, ILogger logger, string projectName)
        {
            var firmalar = configuration.GetSection("IkasAPI").Get<IkasFirmConfig[]>();
            var firmaConfig = firmalar.FirstOrDefault(f => f.ProjectName.ToLower() == projectName.ToLower());

            if (firmaConfig == null)
            {
                logger.LogError($"❌ IkasAPI için {projectName} konfigürasyonu bulunamadı.");
                throw new Exception($"IkasAPI için {projectName} yapılandırması bulunamadı.");
            }

            return firmaConfig;
        }
    }
}
