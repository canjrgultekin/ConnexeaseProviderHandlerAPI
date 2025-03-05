using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IkasAPI.Models;
using IkasAPI.Helper;
using IkasAPI.Enums;

namespace IkasAPI.Services
{
    public class IkasService : IIkasService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IkasService> _logger;

        public IkasService(HttpClient httpClient, IConfiguration configuration, ILogger<IkasService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<object> GetCustomerDataAsync(string projectName,string customerId, string authToken)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);

            var requestBody = new
            {
                query = "query getMyCustomer { getMyCustomer { birthDate email firstName fullName gender id lastName phone } }",
                variables = new { }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",authToken);

            var response = await _httpClient.PostAsync($"{firmaConfig.BaseUrl}/api/sf/graphql?op=getMyCustomer", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(jsonResponse);
        }

        public async Task<object> HandleIkasRequestAsync(IkasRequestDto request)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, request.ProjectName);
            if (!Enum.TryParse(request.ActionType, true, out ActionType actionType))
            {
                throw new ArgumentException("Geçersiz ActionType");
            }
            var requestBody = new
            {
                query = actionType.GetActionTypeString() switch
                {
                    "add_to_cart" or "remove_to_cart" => "query getCartById { getCartById { cartId itemCount totalFinalPrice currencyCode } }",
                    "checkout" => "query getCustomerOrders { getCustomerOrders { orderNumber totalPrice status } }",
                    "add_favorite_product" => "query listFavoriteProducts { listFavoriteProducts { productId price createdAt } }",
                    _ => throw new ArgumentException("Geçersiz ActionType")
                },
                variables = new { }
            };
            var operation = actionType.GetActionTypeString() switch
            {
                "add_to_cart" or "remove_to_cart" => "getCartById",
                "checkout" => "getCustomerOrders",
                "add_favorite_product" => "listFavoriteProducts",
                _ => throw new ArgumentException("Geçersiz ActionType")
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AuthToken);

            var response = await _httpClient.PostAsync($"{firmaConfig.BaseUrl}/api/sf/graphql?op={operation}", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(jsonResponse);
        }
    }
}
