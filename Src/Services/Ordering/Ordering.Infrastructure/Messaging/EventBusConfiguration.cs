using EventBus.Messages.Common;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.EventBusConsumers;
using System;

namespace Ordering.Infrastructure.Messaging
{
    public static class EventBusConfiguration
    {
        public static void ConfigureRabbitMq(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.AddConsumer<BasketCheckoutConsumer>();
                config.AddConsumer<OrderStatusUpdatedConsumer>();

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
                });
            });
        }

        //public static IBusControl ConfigureBus(IServiceProvider provider, Action<IRabbitMqBusFactoryConfigurator, IRabbitMqHost>
        // registrationAction = null)
        //{
        //    return Bus.Factory.CreateUsingRabbitMq(config =>
        //    {
        //        var configuration = provider.GetRequiredService<IConfiguration>();
        //        config.Host(configuration["EventBusSettings:HostAddress"]);

        //        config.ReceiveEndpoint(EventBusConstants.OrderApiQueue, endpoint =>
        //        {
        //            endpoint.UseMessageRetry(r =>
        //            {
        //                r.Ignore<ArgumentNullException>();
        //                r.Interval(3, TimeSpan.FromSeconds(5));
        //            });

        //            endpoint.Consumer<OrderStatusUpdatedConsumer>();

        //            // use the outbox to prevent duplicate events from being published
        //            endpoint.UseInMemoryOutbox();
        //        });
        //    });
        //}
    }
}
