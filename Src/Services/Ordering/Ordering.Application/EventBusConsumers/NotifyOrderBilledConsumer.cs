﻿using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Services;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class NotifyOrderBilledConsumer : IConsumer<NotifyOrderBilled>
    {
        private readonly ILogger<NotifyOrderBilledConsumer> _logger;
        private readonly IMediator _mediator;
        private readonly ITokenValidationService _tokenValidationService;

        public NotifyOrderBilledConsumer(ILogger<NotifyOrderBilledConsumer> logger, IMediator mediator, ITokenValidationService tokenValidationService)
        {
            _logger = logger;
            _mediator = mediator;
            _tokenValidationService = tokenValidationService;
        }

        public async Task Consume(ConsumeContext<NotifyOrderBilled> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order id: {OrderId} billed notification", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(NotifyOrderBilledConsumer));
                return;
            }

            var command = new OrderPaidCommand
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId
            };

            await _mediator.Send(command);
        }
    }
}
