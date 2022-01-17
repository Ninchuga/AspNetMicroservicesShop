using Delivery.API.Services;
using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Delivery.API.Consumers
{
    public class DispatchOrderConsumer : IConsumer<DispatchOrder>
    {
        private readonly ILogger<DispatchOrderConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;

        public DispatchOrderConsumer(ILogger<DispatchOrderConsumer> logger, IConfiguration configuration, 
            ITokenValidationService tokenValidationService, ITokenExchangeService tokenExchangeService)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
        }

        public async Task Consume(ConsumeContext<DispatchOrder> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Dispatch Order event received for the order id: {OrderId}", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(DispatchOrderConsumer));
                return;
            }

            await Task.Delay(10000); // wait 10 seconds

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderSagaOrchestrator"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderSagaOrchestrator"],
                context.Message.SecurityContext.AccessToken);

            var orderDispatched = new OrderDispatched
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DispatchTime = DateTime.UtcNow
            };
            orderDispatched.SecurityContext.AccessToken = accessToken;

            await context.Publish(orderDispatched);
        }
    }
}
