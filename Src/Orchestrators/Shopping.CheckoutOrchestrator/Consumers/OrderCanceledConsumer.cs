using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderCanceledConsumer : IConsumer<OrderCanceled>
    {
        private readonly ILogger<OrderCanceledConsumer> _logger;

        public OrderCanceledConsumer(ILogger<OrderCanceledConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCanceled> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} was canceled.", context.Message.OrderId);

            var rollbackOrderPayment = new RollbackOrderPayment
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                OrderCancelDateTime = context.Message.OrderCancelDateTime,
                OrderTotalPrice = context.Message.OrderTotalPrice,
                CustomerUsername = context.Message.CustomerUsername,
                PaymentCardNumber = context.Message.PaymentCardNumber
            };

            await context.Publish(rollbackOrderPayment);
        }
    }
}
