using AutoMapper;
using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Services;
using System.Threading.Tasks;

namespace Ordering.Application.EventBusConsumers
{
    public class OrderStatusUpdatedConsumer : IConsumer<OrderStatusUpdated>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<OrderStatusUpdatedConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;

        public OrderStatusUpdatedConsumer(IMapper mapper, IMediator mediator,
            ILogger<OrderStatusUpdatedConsumer> logger, ITokenValidationService tokenValidationService)
        {
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            _tokenValidationService = tokenValidationService;
        }

        public async Task Consume(ConsumeContext<OrderStatusUpdated> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);

            _logger.LogInformation("{Consumer} received a message for the order id: {OrderId}", nameof(OrderStatusUpdatedConsumer), context.Message.OrderId);

            //var messageReceivedAt = DateTime.UtcNow;
            //if (!await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, messageReceivedAt))
            //{
            //    _logger.LogInformation("Access Token received by {Consumer} is not valid.", nameof(OrderStatusUpdatedConsumer));

            //    // don't throw exception as that will result in the message not being regarded as handled
            //    return;
            //}

            var command = _mapper.Map<UpdateOrderStatusCommand>(context.Message);
            await _mediator.Send(command);

            _logger.LogInformation("{Event} consumed successfully.", nameof(OrderStatusUpdated));
        }
    }
}
