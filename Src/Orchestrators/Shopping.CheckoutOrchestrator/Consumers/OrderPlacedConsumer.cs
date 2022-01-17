using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Services;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlaced>
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, ITokenValidationService tokenValidationService, ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} placed.", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(OrderPlacedConsumer));
                return;
            }

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:PaymentApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:PaymentApi"],
                context.Message.SecurityContext.AccessToken);

            var billOrder = new BillOrder
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                OrderCreationDate = context.Message.OrderCreationDate,
                PaymentCardNumber = context.Message.PaymentCardNumber,
                OrderTotalPrice = context.Message.OrderTotalPrice,
                CustomerUsername = context.Message.CustomerUsername
            };
            billOrder.SecurityContext.AccessToken = accessToken;

            await context.Publish(billOrder);
        }
    }
}
