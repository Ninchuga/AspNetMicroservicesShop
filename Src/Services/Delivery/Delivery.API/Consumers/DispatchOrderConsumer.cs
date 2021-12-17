using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Delivery.API.Consumers
{
    public class DispatchOrderConsumer : IConsumer<DispatchOrder>
    {
        private readonly ILogger<DispatchOrderConsumer> _logger;

        public DispatchOrderConsumer(ILogger<DispatchOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DispatchOrder> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Dispatch Order event received for the order id: {OrderId}", context.Message.OrderId);

            await Task.Delay(10000); // wait 10 seconds

            var orderDispatched = new OrderDispatched
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DispatchTime = DateTime.UtcNow
            };

            await context.Publish(orderDispatched);
        }
    }
}
