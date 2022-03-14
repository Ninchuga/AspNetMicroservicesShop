using Basket.API.Entities;
using System;
using System.Threading.Tasks;

namespace Basket.API.Services.Basket
{
    public interface IBasketService
    {
        Task<ShoppingBasket> GetUserBasketAndCheckForItemsDiscount(Guid userId);
        Task<ShoppingBasket> UpsertBasket(ShoppingBasket basket);
        Task DeleteBasket(Guid userId);
        Task<ShoppingBasket> DeleteBasketItem(Guid userId, string itemId);
        Task AddItemToBasket(Guid userId, ShoppingBasketItem item);
    }
}