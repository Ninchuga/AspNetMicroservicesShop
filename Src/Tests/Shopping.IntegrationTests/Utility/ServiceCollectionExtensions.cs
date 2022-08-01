using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ordering.Application.HubConfiguration;
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

        public static void RemoveService<T>(this IServiceCollection services) where T : class
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
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

        public static void AddSignalRMockServices(this IServiceCollection services)
        {
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var hubContext = new Mock<IHubContext<OrderStatusHub>>();
            hubContext.Setup(x => x.Clients).Returns(() => mockClients.Object);

            services.AddTransient(provider => hubContext.Object);
        }
    }
}
