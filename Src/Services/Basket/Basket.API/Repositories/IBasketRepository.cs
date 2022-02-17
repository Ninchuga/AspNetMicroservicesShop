using Basket.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingBasket> GetBasket(Guid userId);
        Task<ShoppingBasket> UpdateBasket(ShoppingBasket basket);
        Task DeleteBasket(Guid userId);
    }
}
