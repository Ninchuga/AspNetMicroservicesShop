using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.API.Consumers;
using System;

namespace Payment.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMassTransitWithAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.AddConsumer<BillOrderConsumer>();
                config.AddConsumer<RollbackOrderPaymentConsumer>();

                config.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

                    cfg.Send<OrderPaid>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

                    cfg.SubscriptionEndpoint<BillOrder>("bill-order-consumer", e =>
                    {
                        e.ConfigureConsumer<BillOrderConsumer>(context);

                        // use the outbox to prevent duplicate events from being published
                        e.UseInMemoryOutbox();
                    });

                    cfg.SubscriptionEndpoint<RollbackOrderPayment>("rollback-order-payment-consumer", e =>
                    {
                        e.ConfigureConsumer<RollbackOrderPaymentConsumer>(context);
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
                config.AddConsumer<BillOrderConsumer>();
                config.AddConsumer<RollbackOrderPaymentConsumer>();

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(configuration["EventBusSettings:HostAddress"]);

                    cfg.ReceiveEndpoint(EventBusConstants.OrderBillingQueue, endpoint =>
                    {
                        endpoint.ConfigureConsumer<BillOrderConsumer>(ctx);
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Interval(3, TimeSpan.FromMilliseconds(200));
                            r.Ignore<ArgumentNullException>();
                            r.Handle<InvalidOperationException>();
                        });

                        // use the outbox to prevent duplicate events from being published
                        endpoint.UseInMemoryOutbox();
                    });

                    cfg.ReceiveEndpoint(EventBusConstants.OrderBillingRollbackQueue, endpoint =>
                    {
                        endpoint.ConfigureConsumer<RollbackOrderPaymentConsumer>(ctx);
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Interval(3, TimeSpan.FromMilliseconds(200));
                            r.Ignore<ArgumentNullException>();
                            r.Handle<InvalidOperationException>();
                        });
                    });
                });
            });

            // Used to start the bus
            services.AddMassTransitHostedService();
        }
    }
}
