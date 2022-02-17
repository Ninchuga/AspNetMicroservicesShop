using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<BasketService> _logger;

        public BasketService(
            DiscountGrpcService discountGrpcService, 
            IBasketRepository basketRepository, ILogger<BasketService> logger)
        {
            _discountGrpcService = discountGrpcService;
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task<ShoppingBasket> GetBasketBy(Guid userId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            if(basket == null)
                return new ShoppingBasket(userId);

            foreach (var item in basket.Items)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                if (coupon == null)
                    continue;

                item.PriceWithDiscount = item.Price - coupon.Amount;
                item.Discount = coupon.Amount;
            }

            return basket;
        }

        public async Task<ShoppingBasket> UpdateBasket(ShoppingBasket basket)
        {
            return await _basketRepository.UpdateBasket(basket);
        }

        public async Task DeleteBasket(Guid userId) =>
            await _basketRepository.DeleteBasket(userId);

        public async Task<ShoppingBasket> DeleteBasketItem(Guid userId, string itemId)
        {
            var basket = await GetBasketBy(userId);
            var itemToDelete = basket.Items.FirstOrDefault(item => item.ProductId.Equals(itemId));
            basket.Items.Remove(itemToDelete);

            return await UpdateBasket(basket);
        }

        public async Task AddItemToBasket(Guid userId, ShoppingBasketItem item)
        {
            var userBasket = await _basketRepository.GetBasket(userId);
            if (userBasket == null)
                userBasket = new ShoppingBasket(userId);

            ShoppingBasketItem existingItem = userBasket.Items.FirstOrDefault(existing => existing.ProductId.Equals(item.ProductId));
            if(existingItem == null)
            {
                userBasket.Items.Add(item);
            }
            else
            {
                existingItem.Quantity += item.Quantity;
                existingItem.Price += item.Price * item.Quantity;
            }

            await UpdateBasket(userBasket);
        }
    }
}
