using TicimaxAPI.Models;
using TicimaxAPI.Helper;
using TicimaxAPI.Enums;

namespace TicimaxAPI.Services
{
    public class TicimaxService : ITicimaxService
    {
        private readonly TicimaxWcfClient _ticimaxWcfClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TicimaxService> _logger;

        public TicimaxService(TicimaxWcfClient ticimaxWcfClient, IConfiguration configuration, ILogger<TicimaxService> logger)
        {
            _ticimaxWcfClient = ticimaxWcfClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<object> HandleTicimaxRequestAsync(TicimaxRequestDto request)
        {
            var firmaConfig = Utils.GetFirmaConfig(_configuration, _logger, request.ProjectName); // 🔥 ProjectName bazlı konfig alma
            if (!Enum.TryParse(request.ActionType, true, out ActionType actionType))
            {
                throw new ArgumentException("Geçersiz ActionType");
            }

            var data = actionType.GetActionTypeString() switch
            {
                "add_to_cart" or "remove_to_cart" => await _ticimaxWcfClient.GetSepet(firmaConfig, request.CustomerId),
                "checkout" => await _ticimaxWcfClient.GetSiparis(firmaConfig, request.CustomerId),
                "add_favorite_product" => await _ticimaxWcfClient.GetFavoriUrunler(firmaConfig, request.CustomerId),
                _ => throw new ArgumentException("Geçersiz ActionType")
            };

            return data;
        }

        public async Task<object> GetCustomerDataAsync(string projectName, string sessionId, string customerId)
        {
            var firmaConfig = Utils.GetFirmaConfig(_configuration, _logger, projectName); // 🔥 Firma yapılandırmasını al
            var data = await _ticimaxWcfClient.GetCustomerData(firmaConfig, customerId);
            return data;
        }
    }
}
