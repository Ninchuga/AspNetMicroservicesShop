using AutoMapper;
using EventBus.Messages.Checkout;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.API.Helpers;
using Ordering.Application.Features.Orders.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.API.EventBusConsumer
{
    public class OrderStatusUpdatedConsumer : IConsumer<OrderStatusUpdated>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<OrderStatusUpdatedConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderStatusUpdatedConsumer(IMapper mapper, IMediator mediator,
            ILogger<OrderStatusUpdatedConsumer> logger, ITokenValidationService tokenValidationService, IPublishEndpoint publishEndpoint)
        {
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderStatusUpdated> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);

            _logger.LogInformation("{Consumer} received a message for the user id: {UserId}", nameof(OrderStatusUpdatedConsumer), context.Message.UserId);

            var messageReceivedAt = DateTime.UtcNow;
            if (!await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, messageReceivedAt))
            {
                _logger.LogInformation("Access Token received by {Consumer} is not valid.", nameof(OrderStatusUpdatedConsumer));

                // don't throw exception as that will result in the message not being regarded as handled
                return;
            }

            _logger.LogInformation("{Event} consumed successfully.", nameof(OrderStatusUpdated));

            var command = _mapper.Map<UpdateOrderStatusCommand>(context.Message);
            var response = await _mediator.Send(command);
        }
    }
}
