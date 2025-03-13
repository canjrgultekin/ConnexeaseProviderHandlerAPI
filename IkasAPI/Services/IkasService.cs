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

        public async Task<object> GetCustomerDataAsync(string projectName, string customerId, string authToken)
        {
            var firmaConfig = Utils.GetIkasConfig(_configuration, _logger, projectName);

            var requestBody = new
            {
                query = @"
                        query getMyCustomer {
                            getMyCustomer {
                                id
                                firstName
                                lastName
                                fullName
                                email
                                phone
                                birthDate
                                accountStatus
                                deleted
                                orderCount
                                gender
                                phoneSubscriptionStatus
                                isEmailVerified
                                isPhoneVerified
                                preferredLanguage
                                addresses {
                                    addressLine1
                                    addressLine2
                                    city {
                                        name
                                    }
                                    company
                                    country {
                                        name
                                    }
                                    firstName
                                    id
                                    lastName
                                    phone
                                    postalCode
                                    region {
                                        name
                                    }
                                    identityNumber
                                    isDefault
                                    state {
                                        name
                                    }
                                    taxNumber
                                    taxOffice
                                    title
                                    district {
                                        name
                                    }
                                    deleted
                                    createdAt
                                }
                            }
                        }",
                variables = new { }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.PostAsync($"{firmaConfig.BaseUrl}/api/sf/graphql?op=getMyCustomer", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(jsonResponse);
            return data;
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
                    "add_to_cart" or "remove_to_cart" => @"
            query getCartById {
                getCartById {
                    customer {
                        id
                        fullName
                        email
                        phone
                    }
                    itemCount
                    orderLineItems {
                        id
                        variant {
                            id
                            name
                            sku
                            brand {
                                name
                                id
                            }
                            categories {
                                name
                                id
                            }
                            productId
                        }
                        quantity
                        price
                        unitPrice
                        currencyCode
                        discountPrice
                        finalPrice
                        finalUnitPrice
                        updatedAt
                        createdAt
                    }
                }
            }",
                    "checkout" => @"
            query getCustomerOrders {
                getCustomerOrders {
                    customer {
                        id
                        fullName
                        phone
                        email
                    }
                    orderNumber
                    orderLineItems {
                        id
                        variant {
                            id
                            name
                            sku
                            brand {
                                name
                                id
                            }
                            categories {
                                name
                                id
                            }
                            productId
                        }
                        quantity
                        price
                        unitPrice
                        currencyCode
                        discountPrice
                        finalPrice
                        finalUnitPrice
                        updatedAt
                        createdAt
                    }
                    status
                    totalPrice
                    updatedAt
                    deleted
                    createdAt
                    cancelledAt
                }
            }",
                    "add_favorite_product" => @"
            query listFavoriteProducts {
                listFavoriteProducts {
                    productId
                    price
                    createdAt
                    customerId
                    deleted
                    id
                    priceListId
                    updatedAt
                }
            }",
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
            var data = JsonSerializer.Deserialize<object>(jsonResponse);


            // ** FAVORİ ÜRÜNLERDEN PRODUCT ID'LERİ ÇIKARMA **
            if (actionType.GetActionTypeString() == "add_favorite_product")
            {
                var finalResponse = new object();
               
                var favoriteProductsData = JsonSerializer.Deserialize<JsonDocument>(jsonResponse);

                var productIdList = favoriteProductsData.RootElement
                    .GetProperty("data")
                    .GetProperty("listFavoriteProducts")
                    .EnumerateArray()
                    .Select(p => p.GetProperty("productId").GetString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToArray();

                if (productIdList.Length > 0)
                {
                  
                        // ** PRODUCT INFO QUERY'SİNİ ÇALIŞTIRMA **
                        var productInfoRequest = new
                        {
                            query = @"
                                query productInfo($input: SearchInput!) {
                                    searchProducts(input: $input) {
                                        results {
                                            name
                                            brand {
                                                name
                                                id
                                                slug
                                            }
                                            categories {
                                                name
                                                id
                                                slug
                                            }
                                            id
                                            type
                                            averageRating
                                            reviewCount
                                            stars {
                                                count
                                                star
                                            }
                                            variants {
                                                isActive
                                                sku
                                                prices {
                                                    currency
                                                    sellPrice
                                                    unitPrice
                                                    buyPrice
                                                }
                                                baseBundlePrices {
                                                    discountPrice
                                                    buyPrice
                                                    sellPrice
                                                    unitPrice
                                                    currency
                                                    campaignPrice {
                                                        campaignPrice
                                                        campaignId
                                                    }
                                                }
                                            }
                                            deleted
                                            createdAt
                                        }
                                    }
                                }",
                            variables = new
                            {
                                input = new { productIdList = productIdList }
                            }
                        };

                        var productInfoContent = new StringContent(JsonSerializer.Serialize(productInfoRequest), Encoding.UTF8, "application/json");
                        var productInfoResponse = await _httpClient.PostAsync($"{firmaConfig.BaseUrl}/api/sf/graphql?op=searchProducts", productInfoContent);
                        productInfoResponse.EnsureSuccessStatusCode();

                    var productInfoJson = await productInfoResponse.Content.ReadAsStringAsync();
                    var productInfoData = JsonSerializer.Deserialize<JsonDocument>(productInfoJson);

                    // ** FAVORİ ÜRÜNLERLE PRODUCT INFO DATASINI BİRLEŞTİRME **
                    var favoriteProductsList = favoriteProductsData.RootElement
                        .GetProperty("data")
                        .GetProperty("listFavoriteProducts")
                        .EnumerateArray()
                        .Select(fp =>
                        {
                            var productId = fp.GetProperty("productId").GetString();

                            var matchingProductInfo = productInfoData.RootElement
                                .GetProperty("data")
                                .GetProperty("searchProducts")
                                .GetProperty("results")
                                .EnumerateArray()
                                .FirstOrDefault(pi => pi.GetProperty("id").GetString() == productId);

                            string TryGetString(JsonElement element, string propertyName)
                            {
                                if (element.TryGetProperty(propertyName, out var prop))
                                {
                                    // Eğer değer bir sayıysa, string'e dönüştür
                                    if (prop.ValueKind == JsonValueKind.Number)
                                    {
                                        return prop.GetRawText(); // Sayıyı direkt string formatına çevir
                                    }
                                    if (prop.ValueKind == JsonValueKind.String)
                                    {
                                        return prop.GetString();
                                    }
                                }
                                return null;
                            }

                            decimal? TryGetDecimal(JsonElement element, string propertyName)
                            {
                                return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number ? prop.GetDecimal() : (decimal?)null;
                            }

                            bool TryGetBool(JsonElement element, string propertyName)
                            {
                                return element.TryGetProperty(propertyName, out var prop) && (prop.ValueKind == JsonValueKind.True || prop.ValueKind == JsonValueKind.False) ? prop.GetBoolean() : false;
                            }

                            // ** FAVORİ ÜRÜNÜN İÇİNE PRODUCT DETAILS EKLEYEREK YENİDEN OLUŞTURMA **
                            var favoriteProductWithDetails = new
                            {
                                productId = TryGetString(fp, "productId"),
                                price = TryGetDecimal(fp, "price"),
                                createdAt = TryGetString(fp, "createdAt"),
                                customerId = TryGetString(fp, "customerId"),
                                deleted = TryGetBool(fp, "deleted"),
                                id = TryGetString(fp, "id"),
                                priceListId = TryGetString(fp, "priceListId"),
                                updatedAt = TryGetString(fp, "updatedAt"),
                                productDetails = matchingProductInfo.ValueKind != JsonValueKind.Undefined ? (object)matchingProductInfo : null
                            };

                            return favoriteProductWithDetails;
                        })
                        .ToList();

                    // ** FAVORİ ÜRÜNLERİN İÇERİSİNE PRODUCT DETAILS EKLENEREK DÖNÜYORUZ ** 🚀
                    finalResponse = new
                    {
                        data = new
                        {
                            listFavoriteProducts = favoriteProductsList
                        }
                    };
                }
                return finalResponse;
            }
            else
            {
                return data;

            }


        }
    }
}
