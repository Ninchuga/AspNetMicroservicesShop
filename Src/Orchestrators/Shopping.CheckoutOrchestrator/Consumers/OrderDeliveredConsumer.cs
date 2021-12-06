using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderDeliveredConsumer : IConsumer<OrderDelivered>
    {
        private readonly ILogger<OrderDeliveredConsumer> _logger;

        public OrderDeliveredConsumer(ILogger<OrderDeliveredConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDelivered> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} was delivered successfully.", context.Message.OrderId);

            var orderDeliveredNotification = new NotifyOrderDelivered
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DeliveryTime = context.Message.DeliveryTime
            };

            await context.Publish(orderDeliveredNotification);
        }
    }
}
