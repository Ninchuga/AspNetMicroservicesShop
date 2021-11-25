using Microsoft.EntityFrameworkCore;
using Shopping.CheckoutOrchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Persistence
{
    public class OrderSagaStore : IOrderSagaStore
    {
        private readonly OrderSagaContext _orderSagaContext;

        public OrderSagaStore(OrderSagaContext orderSagaContext)
        {
            _orderSagaContext = orderSagaContext;
        }

        public async Task<IReadOnlyCollection<Order>> GetOrders()
        {
            return await _orderSagaContext.Orders.ToListAsync();
        }

        public async Task<Order> GetPlacedOrderBy(Guid orderId)
        {
            return await _orderSagaContext.Orders.FirstOrDefaultAsync(order => order.Id.Equals(orderId));
        }

        public async Task Insert(Order order)
        {
            await _orderSagaContext.AddAsync(order);
            await SaveChanges();
        }

        public async Task<int> SaveChanges()
        {
            return await _orderSagaContext.SaveChangesAsync();
        }

        public void Update(Order order)
        {
            _orderSagaContext.Update(order);
            SaveChanges().GetAwaiter().GetResult();
        }
    }
}
