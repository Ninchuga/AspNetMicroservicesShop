using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.EventBusConsumers;
using System;

namespace Ordering.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMassTransitWithAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.AddConsumer<OrderFailedToBeBilledConsumer>();
                config.AddConsumer<NotifyOrderBilledConsumer>();
                config.AddConsumer<NotifyOrderDispatchedConsumer>();
                config.AddConsumer<NotifyOrderDeliveredConsumer>();
                config.AddConsumer<OrderPlacedFaultConsumer>();

                config.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

                    // Use to send the message to topic...not available with Azure Service Bus Basic Tier
                    cfg.Send<OrderPlaced>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                    cfg.Send<OrderCanceled>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

                    cfg.SubscriptionEndpoint<NotifyOrderBilled>("notify-order-billed-consumer", e =>
                    {
                        e.ConfigureConsumer<NotifyOrderBilledConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<NotifyOrderDispatched>("notify-order-dispatched-consumer", e =>
                    {
                        e.ConfigureConsumer<NotifyOrderDispatchedConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<NotifyOrderDelivered>("notify-order-delivered-consumer", e =>
                    {
                        e.ConfigureConsumer<NotifyOrderDeliveredConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<OrderFailedToBeBilled>("order-failed-to-be-billed-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderFailedToBeBilledConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<Fault<OrderPlaced>>("order-placed-fault-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderPlacedFaultConsumer>(context);
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
                config.AddConsumer<OrderFailedToBeBilledConsumer>();
                config.AddConsumer<NotifyOrderDispatchedConsumer>();
                config.AddConsumer<NotifyOrderBilledConsumer>();
                config.AddConsumer<NotifyOrderDeliveredConsumer>();
                config.AddConsumer<OrderPlacedFaultConsumer>();

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["EventBusSettings:HostAddress"]);

                    cfg.ReceiveEndpoint(EventBusConstants.OrderStatusNotifierQueue, endpoint =>
                    {
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Ignore<ArgumentNullException>();
                            r.Interval(3, TimeSpan.FromSeconds(5));
                        });
                        endpoint.ConfigureConsumer<NotifyOrderBilledConsumer>(context);
                        endpoint.ConfigureConsumer<NotifyOrderDispatchedConsumer>(context);
                        endpoint.ConfigureConsumer<NotifyOrderDeliveredConsumer>(context);
                        endpoint.ConfigureConsumer<OrderPlacedFaultConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(EventBusConstants.RollbackOrderQueue, endpoint =>
                    {
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Ignore<ArgumentNullException>();
                            r.Interval(3, TimeSpan.FromSeconds(5));
                        });
                        endpoint.ConfigureConsumer<OrderFailedToBeBilledConsumer>(context);
                    });
                });
            });

            // Used to start bus
            services.AddMassTransitHostedService();
        }
    }
}
