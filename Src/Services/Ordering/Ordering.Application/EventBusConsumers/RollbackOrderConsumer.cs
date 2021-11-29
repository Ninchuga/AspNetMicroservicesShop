using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class RollbackOrderConsumer : IConsumer<RollbackOrder>
    {
        private readonly ILogger<RollbackOrderConsumer> _logger;
        private readonly IMediator _mediator;

        public RollbackOrderConsumer(ILogger<RollbackOrderConsumer> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<RollbackOrder> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Rolling back order with id: {OrderId}...", context.Message.OrderId);

            var command = new DeleteOrderCommand() { OrderId = context.Message.OrderId };
            await _mediator.Send(command);

            // publish message to basket service to rollback the basket items ???
        }
    }
}
