using Basket.API.Entities;
using System;
using System.Threading.Tasks;

namespace Basket.API.Services.Basket
{
    public interface IBasketService
    {
        Task<ShoppingBasket> GetBasketBy(Guid userId);
        Task<ShoppingBasket> UpdateBasket(ShoppingBasket basket);
        Task DeleteBasket(Guid userId);
        Task<ShoppingBasket> DeleteBasketItem(Guid userId, string itemId);
        Task AddItemToBasket(Guid userId, ShoppingBasketItem item);
    }
}