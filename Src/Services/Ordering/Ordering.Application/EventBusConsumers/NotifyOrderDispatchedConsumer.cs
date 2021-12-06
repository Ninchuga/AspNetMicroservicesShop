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
    public class NotifyOrderDispatchedConsumer : IConsumer<NotifyOrderDispatched>
    {
        private readonly ILogger<NotifyOrderDispatchedConsumer> _logger;
        private readonly IMediator _mediator;

        public NotifyOrderDispatchedConsumer(ILogger<NotifyOrderDispatchedConsumer> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<NotifyOrderDispatched> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order id: {OrderId} dispatched notification", context.Message.OrderId);

            var command = new UpdateOrderStatusCommand
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                OrderStatus = OrderStatus.ORDER_DISPATCHED
            };

            await _mediator.Send(command);
        }
    }
}
