namespace ProviderHandlerAPI.Models
{
    public class ClientResponseDto
    {
        public string CustomerId { get; set; }
        public string SessionId { get; set; }
        public string Provider { get; set; }
        public string ProjectName { get; set; }
        public object CustomerDataById { get; set; }
        public object ServiceDataByActionType { get; set; }
    }
}
