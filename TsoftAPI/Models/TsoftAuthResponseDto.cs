using System.Collections.Generic;

namespace TsoftAPI.Models
{
    public class TsoftAuthResponseDto
    {
        public bool Success { get; set; }
        public List<TsoftAuthData> Data { get; set; }
        public List<TsoftAuthMessage> Message { get; set; }
        public string Summary { get; set; }
    }

    public class TsoftAuthData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Token { get; set; } // 🔥 Alınması gereken token burası
        public string SecretKey { get; set; }
        public string ExpirationTime { get; set; }
        public string Limited { get; set; }
        public string Type { get; set; }
        public string TableId { get; set; }
    }

    public class TsoftAuthMessage
    {
        public int Type { get; set; }
        public string Code { get; set; }
        public int Index { get; set; }
        public string Id { get; set; }
        public string Subid { get; set; }
        public List<string> Text { get; set; }
        public List<string> ErrorField { get; set; }
    }
}
