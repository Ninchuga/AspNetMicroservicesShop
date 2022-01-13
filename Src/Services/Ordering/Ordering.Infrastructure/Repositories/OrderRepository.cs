using Microsoft.EntityFrameworkCore;
using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(OrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<Order> GetOrderBy(Guid orderId)
        {
            return await _orderContext
               .Orders
               .Include(order => order.OrderItems)
               .FirstOrDefaultAsync(order => order.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersBy(Guid userId)
        {
            var orderList = await _orderContext
                .Orders
                .Include(order => order.OrderItems)
                .AsNoTracking()
                .Where(order => order.UserId == userId)
                .ToListAsync();

            return orderList;
        }
    }
}
