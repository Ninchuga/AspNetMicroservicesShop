using Basket.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingCart> GetBasket(Guid userId);
        Task<ShoppingCart> UpdateBasket(ShoppingCart basket);
        Task DeleteBasket(Guid userId);
    }
}
