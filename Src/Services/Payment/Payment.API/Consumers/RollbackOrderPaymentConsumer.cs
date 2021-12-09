using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Consumers
{
    public class RollbackOrderPaymentConsumer : IConsumer<RollbackOrderPayment>
    {
        private readonly ILogger<RollbackOrderPaymentConsumer> _logger;

        public RollbackOrderPaymentConsumer(ILogger<RollbackOrderPaymentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RollbackOrderPayment> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Rollback payment for the order id: {OrderId}", context.Message.OrderId);

            // TODD: Do money rollback to customer account
        }
    }
}
