using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payment.API.Services;
using System.Threading.Tasks;

namespace Payment.API.Consumers
{
    public class BillOrderConsumer : IConsumer<BillOrder>
    {
        private readonly ILogger<BillOrderConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public BillOrderConsumer(ILogger<BillOrderConsumer> logger, ITokenValidationService tokenValidationService, ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<BillOrder> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Bill Order event received for the order id: {OrderId}", context.Message.OrderId);

            //throw new InvalidOperationException("Failed to bill the order");

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(BillOrderConsumer));
                return;
            }

            await Task.Delay(10000); // wait 10 seconds

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderSagaOrchestrator"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderSagaOrchestrator"],
                context.Message.SecurityContext.AccessToken);

            var orderPaid = new OrderPaid
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                OrderCancellationDate = context.Message.OrderCancellationDate,
                OrderCreationDate = context.Message.OrderCreationDate,
                PaymentCardNumber = context.Message.PaymentCardNumber,
                OrderTotalPrice = context.Message.OrderTotalPrice
            };
            orderPaid.SecurityContext.AccessToken = accessToken;
            
            await context.Publish(orderPaid);
        }
    }
}
