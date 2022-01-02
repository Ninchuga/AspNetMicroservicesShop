using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Infrastructure.Extensions;
using Ordering.Infrastructure.Mail;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repositories;
using System;

namespace Ordering.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("OrderingConnectionString"), option =>
                    {
                        // EF connection resiliency
                        option.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }));

            // loaded once per HTTP request
            services.AddScoped<IOrderContext>(provider => provider.GetService<OrderContext>());

            services.AddTransient(typeof(IRepository<>), typeof(RepositoryBase<>));
            services.AddTransient<IOrderRepository, OrderRepository>();

            services.Configure<EmailSettings>(c => configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // Healtcheks for the API
            services.AddHealthChecks()
                .AddDbContextCheck<OrderContext>("Order Context", tags: new string[] { "order context ready", "sql server" }) //checks the underlying context connection, calls CanConnectAsync
                .AddRabbitMQ(configuration["EventBusSettings:HostAddress"], null, "Rabbit MQ", HealthStatus.Degraded, tags: new string[] { "rabbit ready" }, TimeSpan.FromSeconds(5));

            bool useAzureServiceBus = configuration.GetValue<bool>("Azure:UseAzureServiceBus");
            if (useAzureServiceBus)
                services.ConfigureMassTransitWithAzureServiceBus(configuration);
            else
                services.ConfigureMassTransitWithRabbitMQ(configuration);

            return services;
        }
    }
}
