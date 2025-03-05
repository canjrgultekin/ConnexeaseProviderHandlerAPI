namespace ConnexeaseProviderHandlerAPI.Models
{
    public class TsoftAuthResponseDto
    {
        public string AccessToken { get; set; } // Tsoft API’den dönen token
        public string TokenType { get; set; } // "Bearer"
        public int ExpiresIn { get; set; } // Token süresi (saniye cinsinden)
    }
}
