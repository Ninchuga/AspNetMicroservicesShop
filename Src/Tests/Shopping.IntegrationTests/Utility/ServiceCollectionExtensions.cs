using MassTransit.AspNetCoreIntegration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Ordering.Application.HubConfiguration;
using System;
using System.Linq;

namespace Shopping.IntegrationTests.Utility
{
    public static class ServiceCollectionExtensions
    {
        public static void RemoveDbContext<T>(this IServiceCollection services) where T : DbContext
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<T>));
            if (descriptor != null) services.Remove(descriptor);
        }

        public static void EnsureDbCreated<T>(this IServiceCollection services) where T : DbContext
        {
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<T>();
            context.Database.EnsureCreated();
        }

        public static void RemoveDistributedCache<T>(this IServiceCollection services) where T : IDistributedCache
        {
            var descriptor = services.SingleOrDefault(d => d.ImplementationType == typeof(T));
            if (descriptor != null) services.Remove(descriptor);
        }

        public static void RemoveService<T>(this IServiceCollection services) where T : class
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
            if (descriptor != null) services.Remove(descriptor);
        }

        public static void AddSignalRMockServices(this IServiceCollection services)
        {
            Mock<IHubClients> mockClients = new();
            Mock<IClientProxy> mockClientProxy = new();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var hubContext = new Mock<IHubContext<OrderStatusHub>>();
            hubContext.Setup(x => x.Clients).Returns(() => mockClients.Object);

            services.AddTransient(provider => hubContext.Object);
        }

        public static void RemoveMassTransit(this IServiceCollection services)
        {
            var massTransitHostedService = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) &&
                    d.ImplementationFactory != null &&
                    d.ImplementationFactory.Method.ReturnType == typeof(MassTransitHostedService)
                );
            services.Remove(massTransitHostedService);

            var descriptors = services.Where(d =>
                   d.ServiceType.Namespace.Contains("MassTransit", StringComparison.OrdinalIgnoreCase))
                                      .ToList();
            foreach (var d in descriptors)
            {
                services.Remove(d);
            }
        }
    }
}
