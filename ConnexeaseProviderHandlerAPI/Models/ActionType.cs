namespace ConnexeaseProviderHandlerAPI.Models
{
    public enum ActionType
    {
        AddToCart,          // Ürün sepete eklendi
        RemoveFromCart,     // Ürün sepetten çıkarıldı
        AddFavoriteProduct, // Ürün favorilere eklendi
        Checkout            // Satın alma işlemi gerçekleşti
    }
}
