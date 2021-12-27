using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class OrderPlacedFaultConsumer : IConsumer<Fault<OrderPlaced>>
    {
        private readonly ILogger<OrderPlacedFaultConsumer> _logger;

        public OrderPlacedFaultConsumer(ILogger<OrderPlacedFaultConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Fault<OrderPlaced>> context)
        {
            var faultMessage = context.Message.Message;
            var exceptions = context.Message.Exceptions;
            var orderId = context.Message.Message.OrderId;
            var correlationId = context.Message.Message.CorrelationId;

            using var loggerScope = _logger.BeginScope("{CorrelationId}", correlationId);
            _logger.LogError("Event {EventName} failed with message: {FaultMessage} for the order with id: {OrderId}", nameof(BillOrder), faultMessage, orderId);

            await Task.CompletedTask;
        }
    }
}
