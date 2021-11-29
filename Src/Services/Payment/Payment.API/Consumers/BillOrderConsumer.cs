using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Bill order...reserve amount

            // TODO: Used for testing, remove it afterwards
            //var orderCanceled = new OrderFailedToBeBilled
            //{
            //    OrderId = context.Message.OrderId,
            //    CorrelationId = context.Message.CorrelationId,
            //    OrderCancelDateTime = DateTime.UtcNow
            //};

            //await context.Publish(orderCanceled);

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

    public class BillOrderConsumerDefinition : ConsumerDefinition<BillOrderConsumer>
    {
        public BillOrderConsumerDefinition()
        {
            // override the default endpoint name
            //EndpointName = "order-service";

            // limit the number of messages consumed concurrently
            // this applies to the consumer only, not the endpoint
            ConcurrentMessageLimit = 8;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<BillOrderConsumer> consumerConfigurator)
        {
            // configure message retry with millisecond intervals
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));

            // use the outbox to prevent duplicate events from being published
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
