using Azure.Messaging.ServiceBus;
using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.EventBusConsumers;
using Ordering.Application.Publishers;
using System;

namespace Ordering.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            bool useAzureServiceBus = configuration.GetValue<bool>("UseAzureServiceBus");
            if (useAzureServiceBus)
            {
                //services.AddScoped<ServiceBusSender>();
                //services.AddScoped<OrderPlacedPublisher>();
                //services.AddScoped<ServiceBusTopicSender>();

                services.AddMassTransit(config =>
                {
                    config.AddConsumer<OrderFailedToBeBilledConsumer>();
                    config.AddConsumer<NotifyOrderBilledConsumer>();
                    config.AddConsumer<NotifyOrderDispatchedConsumer>();
                    config.AddConsumer<NotifyOrderDeliveredConsumer>();

                    config.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

                        cfg.Send<OrderPlaced>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<OrderCanceled>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

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

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });

                        cfg.ReceiveEndpoint(EventBusConstants.RollbackOrderQueue, endpoint =>
                        {
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Ignore<ArgumentNullException>();
                                r.Interval(3, TimeSpan.FromSeconds(5));
                            });
                            endpoint.ConfigureConsumer<OrderFailedToBeBilledConsumer>(context);

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });
                    });
                });
            }
            else
            {
                services.AddMassTransit(config =>
                {
                    //config.AddConsumer<BasketCheckoutConsumer>();
                    config.AddConsumer<OrderFailedToBeBilledConsumer>();
                    config.AddConsumer<NotifyOrderDispatchedConsumer>();
                    config.AddConsumer<NotifyOrderBilledConsumer>();
                    config.AddConsumer<NotifyOrderDeliveredConsumer>();

                    config.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(configuration["EventBusSettings:HostAddress"]);

                        //cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, endpoint =>
                        //{
                        //    endpoint.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
                        //});

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

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });

                        
                        cfg.ReceiveEndpoint(EventBusConstants.RollbackOrderQueue, endpoint =>
                        {
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Ignore<ArgumentNullException>();
                                r.Interval(3, TimeSpan.FromSeconds(5));
                            });
                            //endpoint.ConfigureConsumer<RollbackOrderConsumer>(context);
                            endpoint.ConfigureConsumer<OrderFailedToBeBilledConsumer>(context);

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });
                    });
                });

                services.AddMassTransitHostedService(); // used for health check
            }
        }
    }
}
