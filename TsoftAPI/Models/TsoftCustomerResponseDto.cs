namespace TsoftAPI.Models
{
    public class TsoftCustomerResponseDto
    {
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; } // getCart servisi için gerekli
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
