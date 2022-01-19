using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shopping.HostCustomization;
using Shopping.OrderSagaOrchestrator.Persistence;

namespace Shopping.OrderSagaOrchestrator.Extensions
{
    public static class HostExtensions
    {
        public static IHost EligibleForDatabaseMigration(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetService<IConfiguration>();

                // Migrate only if Azure Service Bus is not used
                // otherwise Azure Service Bus session will be used for saga state persistence
                if (!configuration.GetValue<bool>("UseAzureServiceBus"))
                {
                    host.MigrateDatabase<OrderSagaContext>((context, service) =>
                    {
                        var logger = service.GetService<ILogger<OrderSagaContextSeed>>();
                        OrderSagaContextSeed.SeedAsync(context, logger).Wait();
                    });
                }

                return host;
            }
        }
    }
}
