using System;
using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public class TicimaxService : IProviderService
    {
        public async Task<string> ProcessRequestAsync(ClientRequestDto request)
        {
            Console.WriteLine($"Ticimax - İşlem: {request.ActionType}");

            return request.ActionType switch
            {
                "add_to_cart" => await Task.FromResult("Ticimax: Ürün sepete eklendi."),
                "remove_to_cart" => await Task.FromResult("Ticimax: Ürün sepetten çıkarıldı."),
                "add_favorite_product" => await Task.FromResult("Ticimax: Ürün favorilere eklendi."),
                "checkout" => await Task.FromResult("Ticimax: Satın alma işlemi tamamlandı."),
                _ => await Task.FromResult("Ticimax: Geçersiz işlem.")
            };
        }
    }
}
