using Shopping.Aggregator.Models;
using System;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Contracts
{
    public interface IBasketService
    {
        Task<BasketModel> GetBasket(Guid userId);
    }
}
