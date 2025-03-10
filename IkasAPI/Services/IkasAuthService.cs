using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IkasAPI.Models;
using IkasAPI.Helper;

namespace IkasAPI.Services
{
    public class IkasAuthService : IIkasAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IkasAuthService> _logger;

        public IkasAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<IkasAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetAuthAdminTokenAsync(string projectName)
        {
            var token = "";
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);

            var data = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", firmaConfig.ClientId },
                    { "client_secret", firmaConfig.ClientSecret }
                };
            var formContent = new FormUrlEncodedContent(data);

            var url = firmaConfig.BaseUrl+firmaConfig.TokenPath;
         
            try
            {
                var response = await _httpClient.PostAsync(url, formContent);
                response.EnsureSuccessStatusCode(); // 405 alıyorsanız URL veya method hatalıdır.

                var responseString = await response.Content.ReadAsStringAsync();

                token = Utils.ParseAccessToken(responseString);
                if (string.IsNullOrEmpty(token))
                    throw new Exception("Geçerli bir access_token alınamadı.");
            }
            catch (Exception ex)
            {

                _logger.LogError($"❌ Ikas API AdminToken çağrısı başarısız: {ex.Message}");
                throw;
            }
           
        

           

            return token;
        }
    }
}
