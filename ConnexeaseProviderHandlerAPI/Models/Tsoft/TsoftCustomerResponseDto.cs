using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConnexeaseProviderHandlerAPI.Models.Tsoft
{
    public class TsoftCustomerResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<TsoftCustomerData> Data { get; set; } // 🔥 JSON formatına uygun hale getirildi

        [JsonPropertyName("message")]
        public List<TsoftAuthMessage> Message { get; set; } // 🔥 JSON formatına uygun hale getirildi

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }

    public class TsoftCustomerData
    {
        [JsonPropertyName("CustomerId")]
        public string CustomerId { get; set; }

        [JsonPropertyName("CustomerCode")]
        public string CustomerCode { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Surname")]
        public string Surname { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Gender")]
        public string Gender { get; set; }

        [JsonPropertyName("Phone")]
        public string Phone { get; set; }

        [JsonPropertyName("Mobile")]
        public string Mobile { get; set; }

        [JsonPropertyName("City")]
        public string City { get; set; }

        [JsonPropertyName("Town")]
        public string Town { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [JsonPropertyName("RegistrationPlatform")]
        public string RegistrationPlatform { get; set; }

        [JsonPropertyName("Ip")]
        public string Ip { get; set; }

        [JsonPropertyName("Banned")]
        public string Banned { get; set; }

        [JsonPropertyName("CustomerGroupCode")]
        public string CustomerGroupCode { get; set; }
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
        public List<string> Text { get; set; }

        [JsonPropertyName("errorField")]
        public List<string> ErrorField { get; set; }
    }
}
