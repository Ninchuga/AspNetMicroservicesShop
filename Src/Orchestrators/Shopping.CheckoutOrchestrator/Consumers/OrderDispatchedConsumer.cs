using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Services;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderDispatchedConsumer : IConsumer<OrderDispatched>
    {
        private readonly ILogger<OrderDispatchedConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public OrderDispatchedConsumer(ILogger<OrderDispatchedConsumer> logger, ITokenValidationService tokenValidationService, 
            ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<OrderDispatched> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} was delivered successfully.", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(OrderDispatchedConsumer));
                return;
            }

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderApi"],
                context.Message.SecurityContext.AccessToken);

            var orderDispatchedNotification = new NotifyOrderDispatched
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DispatchTime = context.Message.DispatchTime
            };
            orderDispatchedNotification.SecurityContext.AccessToken = accessToken;

            await context.Publish(orderDispatchedNotification);
        }
    }
}
