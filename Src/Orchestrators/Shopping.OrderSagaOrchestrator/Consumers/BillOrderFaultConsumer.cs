using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Services;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class BillOrderFaultConsumer : IConsumer<Fault<BillOrder>>
    {
        private readonly ILogger<BillOrderFaultConsumer> _logger;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public BillOrderFaultConsumer(ILogger<BillOrderFaultConsumer> logger, ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<Fault<BillOrder>> context)
        {
            var faultMessage = context.Message.Message;
            var exceptions = context.Message.Exceptions;
            var orderId = context.Message.Message.OrderId;
            var correlationId = context.Message.Message.CorrelationId;

            using var loggerScope = _logger.BeginScope("{CorrelationId}", correlationId);
            _logger.LogError("Event {EventName} failed with message: {FaultMessage} for the order with id: {OrderId}", nameof(BillOrder), faultMessage, orderId);

            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderApi"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderApi"],
                context.Message.Message.SecurityContext.AccessToken);

            var orderFailed = new OrderFailedToBeBilled
            {
                CorrelationId = correlationId,
                OrderId = orderId
            };
            orderFailed.SecurityContext.AccessToken = accessToken;

            await context.Publish(orderFailed);
        }
    }
}
