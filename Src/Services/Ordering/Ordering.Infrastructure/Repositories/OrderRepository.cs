using Microsoft.EntityFrameworkCore;
using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(IOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<Order> GetOrderBy(Guid orderId)
        {
            return await _orderContext
               .Set<Order>()
               .AsNoTracking()
               .FirstOrDefaultAsync(order => order.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersBy(Guid userId)
        {
            var orderList = await _orderContext
                .Set<Order>()
                .AsNoTracking()
                .Where(order => order.UserId == userId)
                .ToListAsync();

            return orderList;
        }
    }
}
