using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Entities;
using Ordering.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            var orders = orderContext.Orders;
            if (!orders.Any())
            {
                orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChanges();
                logger.LogInformation("Seed database associated with context {DbContextName}", nameof(OrderContext));
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            var order = new Order(
                    orderId: Guid.NewGuid(),
                    userId: Guid.NewGuid(),
                    userName: "ninchuga",
                    totalPrice: 550,
                    orderStatus: Domain.Common.OrderStatus.PENDING,
                    orderDate: DateTime.UtcNow,
                    new Address("Nino", "Djukic", Email.From("someemail@hotmail.com"), "Pasterova 24", "SRB", "NS"),
                    PaymentData.From("Nino", "1234566777", false, CVV.From(555)));
            order.AddOrderItem("1990", "IPhone 6s", 250, 10);

            return new List<Order>
            {
                order
            };
        }
    }
}

