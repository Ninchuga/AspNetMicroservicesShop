using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlaced>
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} placed.", context.Message.OrderId);

            var billOrder = new BillOrder
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                OrderCreationDateTime = context.Message.OrderCreationDateTime,
                PaymentCardNumber = context.Message.PaymentCardNumber,
                OrderTotalPrice = context.Message.OrderTotalPrice,
                CustomerUsername = context.Message.CustomerUsername
            };

            await context.Publish(billOrder);
        }
    }
}
