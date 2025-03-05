namespace ConnexeaseProviderHandlerAPI.Models.Tsoft
{
    public class TsoftResponseDto
    {
        public string Status { get; set; } // "Success" veya "Error"
        public string Message { get; set; } // Yanıt mesajı
        public object Data { get; set; } // Tsoft API'den dönen veriler
    }
}
