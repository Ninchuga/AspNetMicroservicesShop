using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Domain.Common;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class NotifyOrderBilledConsumer : IConsumer<NotifyOrderBilled>
    {
        private readonly ILogger<NotifyOrderBilledConsumer> _logger;
        private readonly IMediator _mediator;

        public NotifyOrderBilledConsumer(ILogger<NotifyOrderBilledConsumer> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<NotifyOrderBilled> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order id: {OrderId} billed notification", context.Message.OrderId);

            var command = new OrderPaidCommand
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId
            };

            await _mediator.Send(command);
        }
    }
}
