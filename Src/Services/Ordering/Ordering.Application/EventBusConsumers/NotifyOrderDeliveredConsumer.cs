using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class NotifyOrderDeliveredConsumer : IConsumer<NotifyOrderDelivered>
    {
        private readonly ILogger<NotifyOrderDeliveredConsumer> _logger;
        private readonly IMediator _mediator;

        public NotifyOrderDeliveredConsumer(ILogger<NotifyOrderDeliveredConsumer> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<NotifyOrderDelivered> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order id: {OrderId} billed notification", context.Message.OrderId);

            var command = new UpdateOrderStatusCommand
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                OrderStatus = OrderStatus.ORDER_DELIVERED
            };

            await _mediator.Send(command);
        }
    }
}
