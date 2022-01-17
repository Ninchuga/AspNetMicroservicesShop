using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Services;
using Ordering.Domain.Common;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class OrderFailedToBeBilledConsumer : IConsumer<OrderFailedToBeBilled>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderFailedToBeBilledConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;

        public OrderFailedToBeBilledConsumer(IMediator mediator, ILogger<OrderFailedToBeBilledConsumer> logger, ITokenValidationService tokenValidationService)
        {
            _mediator = mediator;
            _logger = logger;
            _tokenValidationService = tokenValidationService;
        }

        public async Task Consume(ConsumeContext<OrderFailedToBeBilled> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id: {OrderId} failed to be billed with a reason: {Reason}.", 
                context.Message.OrderId, context.Message.Reason);
            _logger.LogInformation("Rolling back order {OrderId} to status {OrderStatus}", context.Message.OrderId, OrderStatus.PENDING);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(OrderFailedToBeBilledConsumer));
                return;
            }

            // Rollback Order to previous state -> PENDING
            var command = new OrderFailedToBeBilledCommand
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId
            };

            await _mediator.Send(command);
        }
    }
}
