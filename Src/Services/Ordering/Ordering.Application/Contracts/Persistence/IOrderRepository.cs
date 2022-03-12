using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ordering.Application.Contracts.Persistence
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IReadOnlyList<Order>> GetOrdersBy(Guid userId);
        Task<Order> GetOrderBy(Guid orderId);
    }
}
