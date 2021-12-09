using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class BillOrderFaultConsumer : IConsumer<Fault<BillOrder>>
    {
        private readonly ILogger<BillOrderFaultConsumer> _logger;

        public BillOrderFaultConsumer(ILogger<BillOrderFaultConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Fault<BillOrder>> context)
        {
            var faultMessage = context.Message.Message;
            var exceptions = context.Message.Exceptions;
            var orderId = context.Message.Message.OrderId;
            var correlationId = context.Message.Message.CorrelationId;

            using var loggerScope = _logger.BeginScope("{CorrelationId}", correlationId);
            _logger.LogError("Event {EventName} failed with message: {FaultMessage} for the order with id: {OrderId}", nameof(BillOrder), faultMessage, orderId);

            var orderFailed = new OrderFailedToBeBilled
            {
                CorrelationId = correlationId,
                OrderId = orderId
            };

            await context.Publish(orderFailed);
        }
    }
}
