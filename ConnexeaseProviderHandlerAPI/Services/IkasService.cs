using System;
using System.Threading.Tasks;
using ConnexeaseProviderHandlerAPI.Models;

namespace ConnexeaseProviderHandlerAPI.Services
{
    public class IkasService : IProviderService
    {
        public async Task<string> ProcessRequestAsync(ClientRequestDto request)
        {
            if (string.IsNullOrEmpty(request.AuthToken))
                return "Ikas: AuthToken zorunludur.";

            Console.WriteLine($"Ikas - İşlem: {request.ActionType}");

            return request.ActionType switch
            {
                "add_to_cart" => await Task.FromResult("Ikas: Ürün sepete eklendi."),
                "remove_to_cart" => await Task.FromResult("Ikas: Ürün sepetten çıkarıldı."),
                "add_favorite_product" => await Task.FromResult("Ikas: Ürün favorilere eklendi."),
                "checkout" => await Task.FromResult("Ikas: Satın alma işlemi tamamlandı."),
                _ => await Task.FromResult("Ikas: Geçersiz işlem.")
            };
        }
    }
}
