namespace ConnexeaseProviderHandlerAPI.Models
{
    public class TicimaxResponseDto
    {
        public string Status { get; set; }  // "Success" veya "Error"
        public string Message { get; set; } // API yanıt mesajı
        public object Data { get; set; }    // Ticimax'tan dönen veri
    }
}
