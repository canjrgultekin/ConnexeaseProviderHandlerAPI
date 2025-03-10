using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IkasAPI.Models;
using IkasAPI.Helper;

namespace IkasAPI.Services
{
    public class IkasAdminService : IIkasAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IkasAdminService> _logger;
        private readonly IIkasAuthService _authService;

        public IkasAdminService(HttpClient httpClient, IConfiguration configuration, ILogger<IkasAdminService> logger, IIkasAuthService authService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
        }

        private string CreateGraphQLQuery(string baseQuery, Dictionary<string, string> variables)
        {
            var queryFields = new Dictionary<string, string>
    {
        { "listOrder", @"
            count data {
                id customerId cartId branch { id name }
                billingAddress { addressLine1 addressLine2 city { name id } company country { name id } firstName id identityNumber phone postalCode }
                branchSession { id name } cancelledAt cancelReason checkoutId cartStatus couponCode currencyCode
                currencyRates { rate originalRate code } itemCount isGiftPackage lastActivityDate
                customer { email firstName fullName id isGuestCheckout lastName phone preferredLanguage }
                orderNumber orderedAt status totalPrice orderLineItems { id finalPrice finalUnitPrice discountPrice quantity taxValue unitPrice }
            }"
        },
        { "listProduct", @"
            count data {
                brand { id name }
                categories { id name parentId }
                createdAt deleted dynamicPriceListIds id maxQuantityPerCart name productVolumeDiscountId totalStock type
                variants {
                    isActive stocks { updatedAt stockCount productId id }
                    id unit { amount type }
                    prices { currency priceListId sellPrice discountPrice currencySymbol currencyCode buyPrice }
                }
                productVariantTypes { order variantTypeId variantValueIds }
                attributes { productAttributeId productAttributeOptionId value }
                baseUnit { baseAmount type unitId }
                updatedAt weight brandId categoryIds
            }"
        },
        { "listCustomer", @"
            data {
                email firstName fullName deleted id createdAt addresses {
                    addressLine1 addressLine2 city { name } company country { name } identityNumber phone postalCode
                    region { name } state { name } taxNumber taxOffice title firstName lastName createdAt
                }
                birthDate accountStatus accountStatusUpdatedAt firstOrderDate gender isEmailVerified isPhoneVerified
                lastName phone orderCount note lastPriceListId priceListId subscriptionStatus smsSubscriptionStatus
                registrationSource priceListRules { discountRate priceListId value valueType filters { type valueList } }
                phoneSubscriptionStatus preferredLanguage tagIds updatedAt totalOrderPrice
            }"
        }
    };

            var fields = queryFields.ContainsKey(baseQuery) ? queryFields[baseQuery] : throw new ArgumentException("Geçersiz GraphQL Query");

            var filteredVariables = variables
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .ToList();

            if (!filteredVariables.Any())
            {
                return $"query {baseQuery} {{ {baseQuery} {{ {fields} }} }}";
            }

            var variablesString = $"({string.Join(", ", filteredVariables.Select(kv => $"${kv.Key}: {kv.Value}"))})";
            var paramsString = $"({string.Join(", ", filteredVariables.Select(kv => $"{kv.Key}: ${kv.Key}"))})";
            var query = $"query {baseQuery} {variablesString} {{ {baseQuery} {paramsString} {{ {fields} }} }}";
            return query;
        }

        private async Task<object> MakeGraphQLRequest(string projectName, string query, Dictionary<string, string> variables)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);
            var authToken = await _authService.GetAuthAdminTokenAsync(projectName);

            // 🔥 Eğer `variables` tamamen boşsa `{}` olarak gönder
            var requestBody = new
            {
                query,
                variables = variables.Any() ? variables : new Dictionary<string, string>()
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            _logger.LogInformation($"📡 {projectName} için Ikas API'ye GraphQL isteği gönderiliyor: {jsonBody}");

            var response = await _httpClient.PostAsync($"{firmaConfig.BaseUrl}/api/v1/admin/graphql", content);
            var responseBody = await response.Content.ReadAsStringAsync(); // 🔥 Yanıtı oku

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"❌ {projectName} için Ikas API'ye GraphQL isteği başarısız. Status Code: {response.StatusCode}, Response: {responseBody}");
                throw new Exception($"Ikas API GraphQL isteği başarısız: {response.StatusCode} - {responseBody}");
            }

            _logger.LogInformation($"✅ {projectName} için Ikas API GraphQL yanıtı alındı: {responseBody}");

            return JsonSerializer.Deserialize<object>(responseBody);
        }

        public async Task<object> ListCustomers(string projectName)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);

            var variables = new Dictionary<string, string>
    {
        { "merchantId", $"\"{firmaConfig.MerchantId}\"" }
    };

            var query = CreateGraphQLQuery("listCustomer", variables);
            return await MakeGraphQLRequest(projectName, query, variables);
        }

        public async Task<object> ListProducts(string projectName, string productId = null, string brandId = null)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);

            var variables = new Dictionary<string, string>
    {
        { "listProductId", !string.IsNullOrEmpty(productId) ? $"{{eq: \"{productId}\"}}" : null },
        { "brandId", !string.IsNullOrEmpty(brandId) ? $"{{eq: \"{brandId}\"}}" : null }
    };

            var query = CreateGraphQLQuery("listProduct", variables);
            return await MakeGraphQLRequest(projectName, query, variables);
        }

        public async Task<object> ListOrders(string projectName, string orderId = null, string customerId = null, string customerEmail = null)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);

            var variables = new Dictionary<string, string>
    {
        { "orderedAt", !string.IsNullOrEmpty(orderId) ? $"{{eq: \"{orderId}\"}}" : null },
        { "customerId", !string.IsNullOrEmpty(customerId) ? $"{{eq: \"{customerId}\"}}" : null },
        { "customerEmail", !string.IsNullOrEmpty(customerEmail) ? $"{{eq: \"{customerEmail}\"}}" : null },
        { "listOrderId", !string.IsNullOrEmpty(orderId) ? $"{{eq: \"{orderId}\"}}" : null }
    };

            var query = CreateGraphQLQuery("listOrder", variables);
            return await MakeGraphQLRequest(projectName, query, variables);
        }
    }
}
