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
        public static void ConfigureMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            bool useAzureServiceBus = configuration.GetValue<bool>("UseAzureServiceBus");
            if(useAzureServiceBus)
            {
                //services.AddSingleton<IServiceBusConsumer, OrderPlacedConsumer>();
                services.AddMassTransit(config =>
                {
                    config.AddServiceBusMessageScheduler();

                    config.AddConsumer<OrderPlacedConsumer>();
                    config.AddConsumer<OrderCanceledConsumer>();
                    config.AddConsumer<OrderBilledConsumer>();
                    config.AddConsumer<BillOrderFaultConsumer>();
                    config.AddConsumer<OrderDispatchedConsumer>();
                    config.AddConsumer<OrderDeliveredConsumer>();

                    config.AddSagaStateMachine<OrderStateMachine, OrderStateData, OrderSagaDefinition>()
                        .MessageSessionRepository(); // use Azure Service Bus session for saga state persistence

                    config.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

                        cfg.UseServiceBusMessageScheduler();

                        cfg.Send<BillOrder>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<OrderFailedToBeBilled>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<RollbackOrderPayment>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<DispatchOrder>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<NotifyOrderBilled>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<NotifyOrderDelivered>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));
                        cfg.Send<NotifyOrderDispatched>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

                        // Configure QUEUE
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

                        // Subscribe to OrderPlaced directly on the topic, instead of configuring a queue
                        //cfg.SubscriptionEndpoint<OrderPlaced>("order-placed-consumer", e =>
                        //{
                        //    e.ConfigureConsumer<OrderPlacedConsumer>(context);
                        //    e.ConfigureConsumer<OrderCanceledConsumer>(context);
                        //});

                        //cfg.ConfigureEndpoints(context);
                    });
                });
            }
            else
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
                            repo.ConcurrencyMode = MassTransit.EntityFrameworkCoreIntegration.ConcurrencyMode.Pessimistic;

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
            }

            services.AddMassTransitHostedService(); // used for health check
        }
    }
}
