using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Persistence;
using Shopping.OrderSagaOrchestrator.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator
{
    internal class OrderSagaContextSeed
    {
        public static async Task SeedAsync(OrderSagaContext orderContext, ILogger<OrderSagaContextSeed> logger)
        {
            var orderStates = orderContext.Set<OrderStateData>();
            if (!orderStates.Any())
            {
                orderStates.AddRange(GetPreconfiguredOrderSagaStates());
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", nameof(OrderSagaContext));
            }
        }

        private static IEnumerable<OrderStateData> GetPreconfiguredOrderSagaStates()
        {
            var orderState = new OrderStateData
            {
                CorrelationId = Guid.NewGuid(),
                CurrentState = "OrderPlaced",
                CustomerUsername = "ninchuga",
                OrderCreationDate = DateTime.UtcNow,
                OrderId = Guid.NewGuid(),
                OrderTotalPrice = 550,
                PaymentCardNumber = "123456789"
            };

            return new List<OrderStateData>
            {
                orderState
            };
        }
    }
}