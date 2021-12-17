using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Payment.API.Consumers
{
    public class BillOrderConsumer : IConsumer<BillOrder>
    {
        private readonly ILogger<BillOrderConsumer> _logger;

        public BillOrderConsumer(ILogger<BillOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BillOrder> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Bill Order event received for the order id: {OrderId}", context.Message.OrderId);

            //throw new InvalidOperationException("Failed to bill the order");

            await Task.Delay(10000); // wait 10 seconds

            var orderBilled = new OrderBilled
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                OrderCancelDateTime = context.Message.OrderCancelDateTime,
                OrderCreationDateTime = context.Message.OrderCreationDateTime,
                PaymentCardNumber = context.Message.PaymentCardNumber,
                OrderTotalPrice = context.Message.OrderTotalPrice
            };

            
            await context.Publish(orderBilled);
        }
    }
}
