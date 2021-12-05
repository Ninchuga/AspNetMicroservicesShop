﻿using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.API.Consumers
{
    public class DispatchOrderConsumer : IConsumer<DispatchOrder>
    {
        private readonly ILogger<DispatchOrderConsumer> _logger;

        public DispatchOrderConsumer(ILogger<DispatchOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DispatchOrder> context)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", context.Message.CorrelationId);
            _logger.LogInformation("Dispatch Order event received for the order id: {OrderId}", context.Message.OrderId);

            var orderDispatched = new OrderDispatched
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId,
                DispatchTime = DateTime.UtcNow
            };

            await context.Publish(orderDispatched);
        }
    }
}
