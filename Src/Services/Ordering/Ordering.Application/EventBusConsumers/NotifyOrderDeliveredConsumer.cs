﻿using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Services;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class NotifyOrderDeliveredConsumer : IConsumer<NotifyOrderDelivered>
    {
        private readonly ILogger<NotifyOrderDeliveredConsumer> _logger;
        private readonly IMediator _mediator;
        private readonly ITokenValidationService _tokenValidationService;

        public NotifyOrderDeliveredConsumer(ILogger<NotifyOrderDeliveredConsumer> logger, IMediator mediator, ITokenValidationService tokenValidationService)
        {
            _logger = logger;
            _mediator = mediator;
            _tokenValidationService = tokenValidationService;
        }

        public async Task Consume(ConsumeContext<NotifyOrderDelivered> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order id: {OrderId} billed notification", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(NotifyOrderDeliveredConsumer));
                return;
            }

            var command = new OrderDeliveredCommand(context.Message.CorrelationId, context.Message.OrderId);
            await _mediator.Send(command);
        }
    }
}
