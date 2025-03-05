using System;
using ConnexeaseProviderHandlerAPI.Enums;

namespace ConnexeaseProviderHandlerAPI.Helper
{
    public static class EnumExtensions
    {
        public static string GetActionTypeString(this ActionType actionType)
        {
            return actionType switch
            {
                ActionType.AddToCart => "add_to_cart",
                ActionType.RemoveFromCart => "remove_to_cart",
                ActionType.Checkout => "checkout",
                ActionType.AddFavoriteProduct => "add_favorite_product",
                _ => throw new ArgumentException("Geçersiz ActionType")
            };
        }
        public static string GetProviderTypeString(this ProviderType providerType)
        {
            return providerType switch
            {
                ProviderType.Ikas => "ikas",
                ProviderType.Tsoft => "tsoft",
                ProviderType.Ticimax => "ticimax",
                _ => throw new ArgumentException("Geçersiz ProviderType")
            };
        }
    }
}
