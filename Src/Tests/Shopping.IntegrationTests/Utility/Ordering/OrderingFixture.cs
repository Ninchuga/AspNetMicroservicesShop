using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Infrastructure.Persistence;
using Respawn;
using Respawn.Graph;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Shopping.IntegrationTests.Utility.Ordering
{
    public class OrderingFixture : IDisposable
    {
        private static IConfigurationRoot _configuration;
        private static IServiceScopeFactory _scopeFactory;
        private static Checkpoint _checkpoint;

        public OrderingFixture()
        {
            // place the startup code in here
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Ordering/appsettings.json", optional: false)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            var services = new ServiceCollection();

            var startup = new OrderingTestStartup(_configuration);

            services.AddLogging();
            services.AddSingleton<IConfiguration>(_configuration);

            startup.ConfigureServices(services);

            _scopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();

            // Respawn is a small utility to help in resetting test databases to a clean state.
            // Instead of deleting data at the end of a test or rolling back a transaction,
            // Respawn resets the database back to a clean checkpoint by intelligently deleting data from tables
            _checkpoint = new Checkpoint
            {
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
            };

            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<OrderContext>();

            context.Database.Migrate();
        }

        public async Task ResetDbState()
        {
            await _checkpoint.Reset(_configuration.GetConnectionString("OrderingConnectionString"));
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetService<IMediator>();

            return await mediator.Send(request);
        }

        public void Dispose()
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<OrderContext>();

            context.Dispose();
        }
    }
}
