using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TsoftAPI.Models.Authentication
{
    public class TsoftAuthResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<TsoftAuthData> Data { get; set; } = new List<TsoftAuthData>(); // 🔥 JSON formatına uygun hale getirildi

        [JsonPropertyName("message")]
        public List<TsoftAuthMessage> Message { get; set; } = new List<TsoftAuthMessage>(); // 🔥 JSON formatına uygun hale getirildi

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }

    public class TsoftAuthData
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } // 🔥 `token` alanı ile eşleşiyor

        [JsonPropertyName("secretKey")]
        public string SecretKey { get; set; }

        [JsonPropertyName("expirationTime")]
        public string ExpirationTime { get; set; } // 🔥 ExpirationTime doğrudan string olarak geliyor

        [JsonPropertyName("limited")]
        public string Limited { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("tableId")]
        public string TableId { get; set; }
    }

    public class TsoftAuthMessage
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("subid")]
        public string Subid { get; set; }

        [JsonPropertyName("text")]
        public List<string> Text { get; set; } = new List<string>(); // 🔥 Eğer JSON içinde `null` gelirse hata almamak için boş liste

        [JsonPropertyName("errorField")]
        public List<string> ErrorField { get; set; } = new List<string>(); // 🔥 Eğer JSON içinde `null` gelirse hata almamak için boş liste
    }
}
