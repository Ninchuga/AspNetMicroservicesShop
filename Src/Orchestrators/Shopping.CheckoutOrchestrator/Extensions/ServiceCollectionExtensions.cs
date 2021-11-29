using EventBus.Messages.Common;
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
            services.AddMassTransit(config =>
            {
                config.AddConsumer<BillOrderFaultConsumer>();

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

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(configuration["EventBusSettings:HostAddress"]);

                    cfg.ReceiveEndpoint(EventBusConstants.OrderSagaQueue, endpoint =>
                    {
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Ignore<ArgumentNullException>();
                            r.Interval(3, TimeSpan.FromSeconds(5));
                        });

                        endpoint.StateMachineSaga<OrderStateData>(ctx);
                        endpoint.ConfigureConsumer<BillOrderFaultConsumer>(ctx);

                        // use the outbox to prevent duplicate events from being published
                        endpoint.UseInMemoryOutbox();
                    });
                });
            });
        }
    }
}
