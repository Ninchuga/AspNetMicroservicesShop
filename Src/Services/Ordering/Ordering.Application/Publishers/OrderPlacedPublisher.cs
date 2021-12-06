using Azure.Messaging.ServiceBus;
using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ordering.Application.Publishers
{
    public class OrderPlacedPublisher
    {
        private readonly IConfiguration _configuratin;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;

        public OrderPlacedPublisher(IConfiguration configuratin)
        {
            _configuratin = configuratin;
            _serviceBusClient = new ServiceBusClient(_configuratin["AzureServiceBus:OrderSagaConnectionString"]);
            _serviceBusSender = _serviceBusClient.CreateSender(EventBusConstants.OrderSagaQueue);
        }

        public async Task SendMessage(OrderPlaced payload)
        {
            string messagePayload = JsonSerializer.Serialize(payload);
            ServiceBusMessage message = new ServiceBusMessage(messagePayload);

            await _serviceBusSender.SendMessageAsync(message);
        }
    }
}
