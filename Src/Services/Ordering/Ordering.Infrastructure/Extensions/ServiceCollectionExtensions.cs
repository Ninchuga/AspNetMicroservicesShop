using EventBus.Messages.Common;
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
        public static void ConfigureMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.AddConsumer<BasketCheckoutConsumer>();
                config.AddConsumer<OrderStatusUpdatedConsumer>();
                config.AddConsumer<RollbackOrderConsumer>();

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(configuration["EventBusSettings:HostAddress"]);

                    cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, endpoint =>
                    {
                        endpoint.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
                    });

                    cfg.ReceiveEndpoint(EventBusConstants.OrderStatusUpdateQueue, endpoint =>
                    {
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Ignore<ArgumentNullException>();
                            r.Interval(3, TimeSpan.FromSeconds(5));
                        });
                        endpoint.ConfigureConsumer<OrderStatusUpdatedConsumer>(ctx);

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
                        endpoint.ConfigureConsumer<RollbackOrderConsumer>(ctx);

                        // use the outbox to prevent duplicate events from being published
                        endpoint.UseInMemoryOutbox();
                    });
                });
            });
        }
    }
}
