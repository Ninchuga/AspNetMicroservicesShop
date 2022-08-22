using Basket.API.Entities;
using Basket.API.Repositories;
using Basket.API.Services.Discount;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly IDiscountService _discountService;
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<BasketService> _logger;

        public BasketService(
            IDiscountService discountGrpcService, 
            IBasketRepository basketRepository, ILogger<BasketService> logger)
        {
            _discountService = discountGrpcService;
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task<ShoppingBasket> GetUserBasketAndCheckForItemsDiscount(Guid userId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            if(basket is null)
                return new ShoppingBasket(userId);

            return await CheckForBasketItemsDiscountUpdate(basket);
        }

        private async Task<ShoppingBasket> CheckForBasketItemsDiscountUpdate(ShoppingBasket basket)
        {
            foreach (var item in basket.Items)
            {
                // TODO: Maybe we can store discounts in local cache to reduce the number of calls for each individual item
                var coupon = await _discountService.GetDiscount(item.ProductName);
                if (coupon is null)
                    continue;

                basket.UpdateItemDiscount(item, coupon.Amount);
            }

            if (basket.ShouldUpdateBasket)
                basket = await UpsertBasket(basket);

            return basket;
        }

        public async Task<ShoppingBasket> UpsertBasket(ShoppingBasket basket)
        {
            return await _basketRepository.UpdateBasket(basket);
        }

        public async Task DeleteBasket(Guid userId) =>
            await _basketRepository.DeleteBasket(userId);

        public async Task<ShoppingBasket> DeleteBasketItem(Guid userId, string itemId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            var itemToDelete = basket.Items.FirstOrDefault(item => item.ProductId.Equals(itemId));
            if (itemToDelete == null)
            {
                _logger.LogWarning("Item with id {ItemId} that you trying to remove doesn't exist in user {UserId} basket", itemId, userId);
                return basket;
            }

            basket.Items.Remove(itemToDelete);

            return await UpsertBasket(basket);
        }

        public async Task AddItemToBasket(Guid userId, ShoppingBasketItem item)
        {
            var userBasket = await _basketRepository.GetBasket(userId);
            if (userBasket == null)
                userBasket = new ShoppingBasket(userId);

            userBasket.AddBasketItem(item);

            await UpsertBasket(userBasket);
        }
    }
}
