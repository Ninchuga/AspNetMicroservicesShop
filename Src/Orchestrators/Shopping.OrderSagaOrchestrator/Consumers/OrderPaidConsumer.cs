using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Services;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderPaidConsumer : IConsumer<OrderPaid>
    {
        private readonly ILogger<OrderPaidConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public OrderPaidConsumer(ILogger<OrderPaidConsumer> logger, ITokenValidationService tokenValidationService, 
            ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<OrderPaid> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} paid successfully.", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(OrderPlacedConsumer));
                return;
            }

            await NotifyThatOrderIsPaid(context);
            await PublishDispatchOrderCommand(context);
        }

        private async Task NotifyThatOrderIsPaid(ConsumeContext<OrderPaid> context)
        {
            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderApi"],
                context.Message.SecurityContext.AccessToken);

            var notifyThatOrderIsPaid = new NotifyOrderPaid
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId
            };
            notifyThatOrderIsPaid.SecurityContext.AccessToken = accessToken;

            await context.Publish(notifyThatOrderIsPaid);
        }

        private async Task PublishDispatchOrderCommand(ConsumeContext<OrderPaid> context)
        {
            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:DeliveryApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:DeliveryApi"],
                context.Message.SecurityContext.AccessToken);

            var dispatchOrder = new DispatchOrder
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                OrderCreationDate = context.Message.OrderCreationDate,
                CustomerUsername = context.Message.CustomerUsername,
                OrderTotalPrice = context.Message.OrderTotalPrice
            };
            dispatchOrder.SecurityContext.AccessToken = accessToken;

            await context.Publish(dispatchOrder);
        }
    }
}
