namespace ProviderHandlerAPI.Models
{
    public class CustomerData
    {
        public string CustomerId { get; set; }
        public string SessionId { get; set; }
        public string Provider { get; set; }
        public string ProjectName { get; set; }

        public object Data { get; set; }
    }
}
