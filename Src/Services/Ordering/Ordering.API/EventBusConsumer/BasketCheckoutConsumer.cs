using AutoMapper;
using EventBus.Messages.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.API.Helpers;
using Ordering.Application.Features.Orders.Commands;
using System;
using System.Threading.Tasks;

namespace Ordering.API.EventBusConsumer
{
    public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<BasketCheckoutConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;

        public BasketCheckoutConsumer(IMapper mapper, IMediator mediator, 
            ILogger<BasketCheckoutConsumer> logger, ITokenValidationService tokenValidationService)
        {
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            _tokenValidationService = tokenValidationService;
        }

        public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
        {
            var messageReceivedAt = DateTime.UtcNow;
            if (!await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, messageReceivedAt))
            {
                _logger.LogInformation($"Access Token received by {nameof(BasketCheckoutConsumer)} is not valid.");

                // don't throw exception as that will result in the message not being regarded as handled
                return;
            }

            var command = _mapper.Map<CheckoutOrderCommand>(context.Message);
            await _mediator.Send(command);

            _logger.LogInformation($"{nameof(BasketCheckoutEvent)} consumed successfully. Checkout of the order processed.");
        }
    }
}
