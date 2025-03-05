using System;
using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public class TsoftService : IProviderService
    {
        public async Task<string> ProcessRequestAsync(ClientRequestDto request)
        {
            Console.WriteLine($"Tsoft - İşlem: {request.ActionType}");

            return request.ActionType switch
            {
                "add_to_cart" => await Task.FromResult("Tsoft: Ürün sepete eklendi."),
                "remove_to_cart" => await Task.FromResult("Tsoft: Ürün sepetten çıkarıldı."),
                "add_favorite_product" => await Task.FromResult("Tsoft: Ürün favorilere eklendi."),
                "checkout" => await Task.FromResult("Tsoft: Satın alma işlemi tamamlandı."),
                _ => await Task.FromResult("Tsoft: Geçersiz işlem.")
            };
        }
    }
}
