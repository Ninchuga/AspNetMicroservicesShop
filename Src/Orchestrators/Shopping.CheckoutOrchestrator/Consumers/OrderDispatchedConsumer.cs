using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderDispatchedConsumer : IConsumer<OrderDispatched>
    {
        private readonly ILogger<OrderDispatchedConsumer> _logger;

        public OrderDispatchedConsumer(ILogger<OrderDispatchedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDispatched> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} was delivered successfully.", context.Message.OrderId);

            var orderDeliveredNotification = new NotifyOrderDispatched
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DispatchTime = context.Message.DispatchTime
            };

            await context.Publish(orderDeliveredNotification);
        }
    }
}
