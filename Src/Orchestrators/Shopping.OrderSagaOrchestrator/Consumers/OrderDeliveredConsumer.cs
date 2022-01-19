using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Services;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderDeliveredConsumer : IConsumer<OrderDelivered>
    {
        private readonly ILogger<OrderDeliveredConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public OrderDeliveredConsumer(ILogger<OrderDeliveredConsumer> logger, ITokenValidationService tokenValidationService, 
            ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<OrderDelivered> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} was delivered successfully.", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(OrderDeliveredConsumer));
                return;
            }

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderApi"],
                context.Message.SecurityContext.AccessToken);

            var orderDeliveredNotification = new NotifyOrderDelivered
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DeliveryTime = context.Message.DeliveryTime
            };
            orderDeliveredNotification.SecurityContext.AccessToken = accessToken;

            await context.Publish(orderDeliveredNotification);
        }
    }
}
