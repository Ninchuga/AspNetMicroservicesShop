using Basket.API.DTOs;
using Basket.API.Entities;
using Basket.API.Responses;
using System;
using System.Threading.Tasks;

namespace Basket.API.Services.Basket
{
    public interface IBasketService
    {
        Task<ShoppingCart> GetBasketBy(Guid userId);
        Task<CheckoutBasketResponse> CheckoutBasket(BasketCheckout basketCheckout, Guid userId, string correlationId = null);
        Task<ShoppingCart> UpdateBasket(ShoppingCart basket);
        Task DeleteBasket(Guid userId);
        Task<ShoppingCart> DeleteBasketItem(Guid userId, string itemId);
        Task AddItemToBasket(Guid userId, ShoppingCartItem item);
    }
}