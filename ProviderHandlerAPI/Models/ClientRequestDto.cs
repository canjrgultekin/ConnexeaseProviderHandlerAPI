namespace ProviderHandlerAPI.Models
{
    public class ClientRequestDto
    {
        public string Provider { get; set; } // Ticimax, Tsoft, Ikas
        public string ProjectName { get; set; } // Ör: shulebags.com, evinemama.com, voidtr.com
        public string SessionId { get; set; }
        public string AuthToken { get; set; } // Ikas için zorunlu
        public string CustomerId { get; set; }
        public string ActionType { get; set; } // add_to_cart, remove_to_cart, add_favorite_product, checkout
    }
}
