using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.API.Publishers
{
    public class OrderDeliveredPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderDeliveredPublisher> _logger;
        //private readonly IDeliveryRepository _deliveryRepository; TODO: Implement repo to store order for delivery

        public OrderDeliveredPublisher(IPublishEndpoint publishEndpoint, ILogger<OrderDeliveredPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Publish()
        {
            // TODO: Get CorrelationId from db and other Order related data
            Guid correlationId = Guid.NewGuid();
            Guid orderId = Guid.NewGuid();
            
            using var loggerScope = _logger.BeginScope("{CorrelationId}", correlationId);
            _logger.LogInformation("Order with id: {OrderId} delivered successfully.", orderId);

            var orderDelivered = new OrderDelivered
            {
                CorrelationId = correlationId,
                OrderId = orderId,
                DeliveryTime = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(orderDelivered);
        }
    }
}
