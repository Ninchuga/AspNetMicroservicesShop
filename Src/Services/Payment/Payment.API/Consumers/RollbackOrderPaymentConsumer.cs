using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payment.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Consumers
{
    public class RollbackOrderPaymentConsumer : IConsumer<RollbackOrderPayment>
    {
        private readonly ILogger<RollbackOrderPaymentConsumer> _logger;
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public RollbackOrderPaymentConsumer(ILogger<RollbackOrderPaymentConsumer> logger, ITokenValidationService tokenValidationService, 
            ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<RollbackOrderPayment> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Rollback payment for the order id: {OrderId}", context.Message.OrderId);

            var tokenValidated = await _tokenValidationService.ValidateTokenAsync(context.Message.SecurityContext.AccessToken, context.SentTime.Value);
            if (!tokenValidated)
            {
                _logger.LogError("Access token validation failed in consumer {ConsumerName}", nameof(RollbackOrderPaymentConsumer));
                return;
            }

            // TODD: Do order amount rollback to customer account
        }
    }
}
