using Shopping.CheckoutOrchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Persistence
{
    public interface IOrderSagaStore
    {
        Task<int> SaveChanges();
        void Update(Order order);
        Task Insert(Order order);
        Task<Order> GetPlacedOrderBy(Guid orderId);
        Task<IReadOnlyCollection<Order>> GetOrders();
    }
}
