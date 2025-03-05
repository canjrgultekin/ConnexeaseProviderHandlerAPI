using TsoftAPI.Models;

namespace TsoftAPI.Helper
{
    public static class Utils
    {
        public static TsoftFirmConfig GetFirmaConfig(IConfiguration configuration, ILogger logger, string projectName)
        {
            var firmalar = configuration.GetSection("TsoftAPI").Get<TsoftFirmConfig[]>();
            var firmaConfig = firmalar.FirstOrDefault(f => f.ProjectName.ToLower() == projectName.ToLower());

            if (firmaConfig == null)
            {
                logger.LogError($"❌ TsoftAPI için {projectName} konfigürasyonu bulunamadı.");
                throw new Exception($"TsoftAPI için {projectName} yapılandırması bulunamadı.");
            }

            return firmaConfig;
        }
    }
}
