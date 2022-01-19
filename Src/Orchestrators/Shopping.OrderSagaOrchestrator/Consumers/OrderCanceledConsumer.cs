using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Services;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderCanceledConsumer : IConsumer<OrderCanceled>
    {
        private readonly ILogger<OrderCanceledConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public OrderCanceledConsumer(ILogger<OrderCanceledConsumer> logger, ITokenValidationService tokenValidationService, 
            ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<OrderCanceled> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Order with id {OrderId} was canceled.", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(OrderCanceledConsumer));
                return;
            }

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:PaymentApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:PaymentApi"],
                context.Message.SecurityContext.AccessToken);

            var rollbackOrderPayment = new RollbackOrderPayment
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                OrderCancellationDate = context.Message.OrderCancellationDate,
                OrderTotalPrice = context.Message.OrderTotalPrice,
                CustomerUsername = context.Message.CustomerUsername,
                PaymentCardNumber = context.Message.PaymentCardNumber
            };
            rollbackOrderPayment.SecurityContext.AccessToken = accessToken;

            await context.Publish(rollbackOrderPayment);
        }
    }
}
