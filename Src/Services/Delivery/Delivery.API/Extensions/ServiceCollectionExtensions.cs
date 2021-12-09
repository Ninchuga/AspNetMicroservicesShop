using Delivery.API.Consumers;
using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Delivery.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMassTransitWithAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.AddConsumer<DispatchOrderConsumer>();

                config.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

                    cfg.Send<OrderDelivered>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

                    cfg.SubscriptionEndpoint<DispatchOrder>("dispatch-order-consumer", e =>
                    {
                        e.ConfigureConsumer<DispatchOrderConsumer>(context);

                        // use the outbox to prevent duplicate events from being published
                        e.UseInMemoryOutbox();
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Used to start bus
            services.AddMassTransitHostedService();
        }

        public static void ConfigureMassTransitWithRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.AddConsumer<DispatchOrderConsumer>();

                config.UsingRabbitMq((ctx, config) =>
                {
                    config.Host(configuration["EventBusSettings:HostAddress"]);

                    config.ReceiveEndpoint(EventBusConstants.OrderDeliveryQueue, endpoint =>
                    {
                        endpoint.ConfigureConsumer<DispatchOrderConsumer>(ctx);
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Interval(3, TimeSpan.FromMilliseconds(200));
                            r.Ignore<ArgumentNullException>();
                            r.Handle<InvalidOperationException>();
                        });

                        // use the outbox to prevent duplicate events from being published
                        endpoint.UseInMemoryOutbox();
                    });
                });
            });

            // Used to start the bus
            services.AddMassTransitHostedService();
        }
    }
}
