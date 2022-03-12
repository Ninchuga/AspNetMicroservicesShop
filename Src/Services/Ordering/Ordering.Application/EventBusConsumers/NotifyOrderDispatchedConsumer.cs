using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Services;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class NotifyOrderDispatchedConsumer : IConsumer<NotifyOrderDispatched>
    {
        private readonly ILogger<NotifyOrderDispatchedConsumer> _logger;
        private readonly IMediator _mediator;
        private readonly ITokenValidationService _tokenValidationService;

        public NotifyOrderDispatchedConsumer(ILogger<NotifyOrderDispatchedConsumer> logger, IMediator mediator, ITokenValidationService tokenValidationService)
        {
            _logger = logger;
            _mediator = mediator;
            _tokenValidationService = tokenValidationService;
        }

        public async Task Consume(ConsumeContext<NotifyOrderDispatched> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order id: {OrderId} dispatched notification", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(NotifyOrderDispatchedConsumer));
                return;
            }

            var command = new OrderDispatchedCommand(context.Message.CorrelationId, context.Message.OrderId);
            await _mediator.Send(command);
        }
    }
}
