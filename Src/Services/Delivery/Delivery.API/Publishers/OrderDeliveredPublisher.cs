using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
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
        private readonly IBus _bus;
        private readonly ILogger<OrderDeliveredPublisher> _logger;
        private readonly IConfiguration _configuration;
        //private readonly IDeliveryRepository _deliveryRepository; TODO: Implement repo to store order for delivery

        public OrderDeliveredPublisher(IPublishEndpoint publishEndpoint, ILogger<OrderDeliveredPublisher> logger, IBus bus, IConfiguration configuration)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _bus = bus;
            _configuration = configuration;
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

            if (_configuration.GetValue<bool>("UseAzureServiceBus") && _configuration.GetValue<bool>("IsBasicTierAzureServiceBus"))
            {
                // when using Azure Basic Plan or RabbitMQ queue for sending messages we need to use Send method on the IBus
                var endpoint = await _bus.GetSendEndpoint(new Uri(_configuration["AzureServiceBus:OrderSagaQueue"]));
                await endpoint.Send(orderDelivered);
            }
            else
            {
                // Use to publish events to Azure Service Bus topic or RabbitMQ Exchange
                await _publishEndpoint.Publish(orderDelivered);
            }
        }
    }
}
