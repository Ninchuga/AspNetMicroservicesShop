using Shopping.Aggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Contracts
{
    public interface IOrderService
    {
        Task<IReadOnlyCollection<OrderResponseModel>> GetOrdersBy(Guid userId);
    }
}
