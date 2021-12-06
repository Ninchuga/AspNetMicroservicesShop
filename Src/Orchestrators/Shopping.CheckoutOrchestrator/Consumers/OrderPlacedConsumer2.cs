using Azure.Messaging.ServiceBus;
using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Consumers
{
    public class OrderPlacedConsumer2 : IServiceBusConsumer
    {
        //private readonly IProcessData _processData;
        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _processor;
        private readonly ILogger<OrderPlacedConsumer2> _logger;

        public OrderPlacedConsumer2(IConfiguration configuration, ILogger<OrderPlacedConsumer2> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var connectionString = _configuration["AzureServiceBus:OrderSagaConnectionString"];
            _client = new ServiceBusClient(connectionString);
        }

        public async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            ServiceBusProcessorOptions serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
            };

            _processor = _client.CreateProcessor(EventBusConstants.OrderSagaQueue, serviceBusProcessorOptions);
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync();

            // stop processing 
            //Console.WriteLine("\nStopping the receiver...");
            //await _processor.StopProcessingAsync();
            //Console.WriteLine("Stopped receiving messages");
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            var myPayload = args.Message.Body.ToObjectFromJson<OrderPlaced>();
            //await _processData.Process(myPayload);
            await args.CompleteMessageAsync(args.Message);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");
            _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
            _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
            _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        public async Task CloseQueueAsync()
        {
            await _processor.CloseAsync();
        }

        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}
