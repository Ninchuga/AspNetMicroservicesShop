using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCache;

        public BasketRepository(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }

        public async Task DeleteBasket(Guid userId)
        {
            await _redisCache.RemoveAsync(userId.ToString());
        }

        public async Task<ShoppingCart> GetBasket(Guid userId)
        {
            var basket = await _redisCache.GetStringAsync(userId.ToString());
            if (basket == null)
                return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            await _redisCache.SetStringAsync(basket.UserId.ToString(), JsonConvert.SerializeObject(basket));

            return await GetBasket(basket.UserId);
        }
    }
}
