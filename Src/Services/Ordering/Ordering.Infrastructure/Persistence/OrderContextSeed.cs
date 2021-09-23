using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(IOrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            var orders = orderContext.Set<Order>();
            if (!orders.Any())
            {
                orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChanges();
                logger.LogInformation("Seed database associated with context {DbContextName}", nameof(OrderContext));
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order() {Id = Guid.NewGuid(), UserName = "test seed user", FirstName = "Nino", LastName = "Djukic", 
                    Email = "ninoslav90@hotmail.com", Address = "Pasterova", Country = "Serbia", TotalPrice = 350 }
            };
        }
    }
}
