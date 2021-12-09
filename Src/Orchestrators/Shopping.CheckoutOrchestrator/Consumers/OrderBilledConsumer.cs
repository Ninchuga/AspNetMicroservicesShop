using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderBilledConsumer : IConsumer<OrderBilled>
    {
        private readonly ILogger<OrderBilledConsumer> _logger;

        public OrderBilledConsumer(ILogger<OrderBilledConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderBilled> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} billed successfully.", context.Message.OrderId);

            await NotifyThatOrderWasBilled(context);
            await PublishDispatchOrderCommand(context);
        }

        private async Task NotifyThatOrderWasBilled(ConsumeContext<OrderBilled> context)
        {
            var notifyThatOrderWasBilled = new NotifyOrderBilled
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId
            };

            await context.Publish(notifyThatOrderWasBilled);
        }

        private async Task PublishDispatchOrderCommand(ConsumeContext<OrderBilled> context)
        {
            var dispatchOrder = new DispatchOrder
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                OrderCreationDateTime = context.Message.OrderCreationDateTime,
                CustomerUsername = context.Message.CustomerUsername,
                OrderTotalPrice = context.Message.OrderTotalPrice
            };

            await context.Publish(dispatchOrder);
        }
    }
}
