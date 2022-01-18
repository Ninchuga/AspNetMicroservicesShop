using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping.OrderSagaOrchestrator.Consumers;
using Shopping.OrderSagaOrchestrator.Persistence;
using Shopping.OrderSagaOrchestrator.StateMachine;
using System;
using System.Reflection;

namespace Shopping.OrderSagaOrchestrator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMassTransitWithAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                // Used for MassTransit schedulers
                //config.AddServiceBusMessageScheduler();

                config.AddConsumer<OrderPlacedConsumer>();
                config.AddConsumer<OrderCanceledConsumer>();
                config.AddConsumer<OrderBilledConsumer>();
                config.AddConsumer<BillOrderFaultConsumer>();
                config.AddConsumer<OrderDispatchedConsumer>();
                config.AddConsumer<OrderDeliveredConsumer>();

                config.AddSagaStateMachine<OrderStateMachine, OrderStateData, OrderSagaDefinition>()
                    .MessageSessionRepository(); // use Azure Service Bus session for saga state persistence. Not allowed in Basic tier mode

                config.UsingAzureServiceBus((context, cfg) =>
                {
                    // Used for MassTransit schedulers
                    //cfg.UseServiceBusMessageScheduler();

                    cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

                    // Subscribe directly on the topic, instead of configuring a queue
                    cfg.SubscriptionEndpoint<OrderPlaced>("order-placed-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderPlacedConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<OrderCanceled>("order-canceled-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderCanceledConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<OrderBilled>("order-billed-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderBilledConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<Fault<BillOrder>>("bill-order-fault-consumer", e =>
                    {
                        e.ConfigureConsumer<BillOrderFaultConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<OrderDispatched>("order-dispatched-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderDispatchedConsumer>(context);
                    });

                    cfg.SubscriptionEndpoint<OrderDelivered>("order-delivered-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderDeliveredConsumer>(context);
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
                config.AddConsumer<OrderPlacedConsumer>();
                config.AddConsumer<OrderCanceledConsumer>();
                config.AddConsumer<OrderBilledConsumer>();
                config.AddConsumer<BillOrderFaultConsumer>();
                config.AddConsumer<OrderDispatchedConsumer>();
                config.AddConsumer<OrderDeliveredConsumer>();

                config.AddSagaStateMachine<OrderStateMachine, OrderStateData>()
                    .EntityFrameworkRepository(repo =>
                    {
                        repo.ConcurrencyMode = MassTransit.EntityFrameworkCoreIntegration.ConcurrencyMode.Optimistic;

                        repo.AddDbContext<DbContext, OrderSagaContext>((provider, builder) =>
                        {
                            builder.UseSqlServer(configuration.GetConnectionString("OrderSagaConnectionString"), m =>
                            {
                                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                                m.MigrationsHistoryTable($"__{nameof(OrderSagaContext)}");
                            });
                        });
                    });

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["EventBusSettings:HostAddress"]);

                    // Configure QUEUE (Direct or Fanout)
                    cfg.ReceiveEndpoint(EventBusConstants.OrderSagaQueue, endpoint =>
                    {
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Ignore<ArgumentNullException>();
                            r.Interval(3, TimeSpan.FromSeconds(5));
                        });

                        endpoint.StateMachineSaga<OrderStateData>(context);
                        endpoint.ConfigureConsumer<OrderPlacedConsumer>(context);
                        endpoint.ConfigureConsumer<OrderCanceledConsumer>(context);
                        endpoint.ConfigureConsumer<OrderBilledConsumer>(context);
                        endpoint.ConfigureConsumer<BillOrderFaultConsumer>(context);
                        endpoint.ConfigureConsumer<OrderDispatchedConsumer>(context);
                        endpoint.ConfigureConsumer<OrderDeliveredConsumer>(context);

                        // use the outbox to prevent duplicate events from being published
                        endpoint.UseInMemoryOutbox();
                    });
                });
            });

            services.AddMassTransitHostedService(); // used for health check and for starting and stopping the bus
        }
    }
}
