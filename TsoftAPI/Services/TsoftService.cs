﻿using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TsoftAPI.Authentication;
using TsoftAPI.Models;
using TsoftAPI.Helper;
using TsoftAPI.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using System.Web;
using System;
using Common.Redis;

namespace TsoftAPI.Services
{
    public class TsoftService : ITsoftService
    {
        private readonly HttpClient _httpClient;
        private readonly TsoftAuthService _authService;
        private readonly ILogger<TsoftService> _logger;
        private readonly RedisCacheService _cacheService;
        private readonly IConfiguration _configuration;

        public TsoftService(HttpClient httpClient, IConfiguration configuration, TsoftAuthService authService, ILogger<TsoftService> logger, RedisCacheService cache)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
            _cacheService = cache;
            _configuration = configuration;
        }

        public async Task<object> HandleTsoftRequestAsync(TsoftRequestDto request)
        {
            var cacheKey = $"TsoftCustomerCode:{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";
            string cachedCustomerCode = await _cacheService.GetCacheObjectAsync<string>(cacheKey);
        

            var firmaConfig = Utils.GetFirmaConfig(_configuration, _logger, request.ProjectName); // 🔥 Helper Metot Kullanılıyor
            var token = await _authService.GetAuthTokenAsync(request);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (!Enum.TryParse(request.ActionType, true, out ActionType actionType))
            {
                throw new ArgumentException("Geçersiz ActionType");
            }

            var apiUrl = actionType.GetActionTypeString() switch
            {
                "add_to_cart" => $"{firmaConfig.BaseUrl}/rest1/customer/getCart/{cachedCustomerCode}",
                "remove_to_cart" => $"{firmaConfig.BaseUrl}/rest1/customer/getCart/{cachedCustomerCode}",
                "checkout" => $"{firmaConfig.BaseUrl}/rest1/order/get",
                "add_favorite_product" => $"{firmaConfig.BaseUrl}/rest1/customer/getWishList",
                _ => throw new ArgumentException("Geçersiz ActionType")
            };
            var postData = actionType.GetActionTypeString() switch
            {
                "add_to_cart" => new Dictionary<string, string>
             {
                { "token", token }
            },
                "remove_to_cart" => new Dictionary<string, string>
             {
                { "token", token }
            },
                "checkout" => new Dictionary<string, string>
             {
                { "token", token },
                { "FetchProductData", "true" },
                { "FetchProductDetail", "true" },
                { "FetchPackageContent", "true" },
                { "FetchCustomerData", "true" },
                { "f", $"CustomerId|{request.CustomerId}|equal" }
             },
                "add_favorite_product" => new Dictionary<string, string>
             {
                { "token", token },
                { "CustomerId", request.CustomerId }
              },
                _ => throw new ArgumentException("Geçersiz ActionType")
            };


            try
            {
                var encodedContent = new FormUrlEncodedContent(postData);
                using HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, encodedContent);
                response.EnsureSuccessStatusCode(); // Hata durumlarını yönetir
                string result = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<object>(result);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Tsoft API çağrısı başarısız: {ex.Message}");
                throw;
            }
        }

        public async Task<object> GetCustomerDataAsync(TsoftRequestDto request)
        {
            var firmaConfig = Utils.GetFirmaConfig(_configuration, _logger, request.ProjectName); // 🔥 Helper Metot Kullanılıyor
            var token = await _authService.GetAuthTokenAsync(request);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            string apiUrl = $"{firmaConfig.BaseUrl}/rest1/customer/getCustomerById/{request.CustomerId}";

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, new StringContent($"token={token}", Encoding.UTF8, "application/x-www-form-urlencoded"));
                var status = response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<object>(jsonResponse);
                if (status.IsSuccessStatusCode)
                {
                    var cacheKey = $"TsoftCustomerCode:{request.Provider}:{request.ProjectName}:{request.SessionId}:{request.CustomerId}";
                    var jsonDoc = JsonDocument.Parse(jsonResponse);
                    var dataArray = jsonDoc.RootElement.GetProperty("data");
                    var customerCode =  dataArray.EnumerateArray().FirstOrDefault().GetProperty("CustomerCode").GetString();
                    await _cacheService.SetCacheAsync(cacheKey, customerCode);
                }
                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Tsoft Customer Data alma hatası: {ex.Message}");
                throw;
            }
        }
    }
}
