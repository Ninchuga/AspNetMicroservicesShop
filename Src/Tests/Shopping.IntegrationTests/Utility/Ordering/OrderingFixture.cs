using Microsoft.Extensions.DependencyInjection;
using Ordering.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Ordering.Domain.Entities;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using Xunit;
using DotNet.Testcontainers.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Moq;
using Ordering.Application.Contracts.Infrastrucutre;
using MassTransit;
using Ordering.API;
using Ordering.Application.Services;

namespace Shopping.IntegrationTests.Utility.Ordering
{
    public class OrderingFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Used when running tests without Testcontainer
        //private IConfigurationRoot _configuration;
        //private IServiceScopeFactory _scopeFactory;
        //private Checkpoint _checkpoint;

        private readonly TestcontainerDatabase _dbContainer;

        public OrderingFixture()
        {
            _dbContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Password = "SwN12345678"
            })
            .Build();


            #region Used when running tests without Testcontainer
            // place the startup code in here
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("Ordering/appsettings.json", optional: false)
            //    .AddEnvironmentVariables();

            //_configuration = builder.Build();

            //var services = new ServiceCollection();

            //var startup = new OrderingTestStartup(_configuration);

            //services.AddLogging();
            //services.AddSingleton<IConfiguration>(_configuration);

            //startup.ConfigureServices(services);

            //_scopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();

            // Respawn is a small utility to help in resetting test databases to a clean state.
            // Instead of deleting data at the end of a test or rolling back a transaction,
            // Respawn resets the database back to a clean checkpoint by intelligently deleting data from tables
            //_checkpoint = new Checkpoint
            //{
            //    TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
            //};

            //EnsureDatabase();
            #endregion
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove OrderContext
                services.RemoveDbContext<OrderContext>();

                // Add DB context pointing to test container
                services.AddDbContext<OrderContext>(options => { options.UseSqlServer($"{_dbContainer.ConnectionString}TrustServerCertificate=True;"); });

                // Ensure schema gets created
                services.EnsureDbCreated<OrderContext>();

                services.RemoveService<IEmailService>();
                services.RemoveService<IPublishEndpoint>();
                services.RemoveService<ITokenExchangeService>();

                services.AddScoped(provider => Mock.Of<IEmailService>());
                services.AddTransient(provider => Mock.Of<IPublishEndpoint>());
                services.AddTransient(provider => Mock.Of<ITokenExchangeService>());

                services.AddSignalRMockServices();
            });
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            //using var scope = _scopeFactory.CreateScope();
            var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(request);
        }

        public async Task<Order> GetOrderBy(Guid orderId)
        {
            //using var scope = _scopeFactory.CreateScope();
            var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<OrderContext>();

            return await context.Orders.FirstOrDefaultAsync(order => order.Id.Equals(orderId));
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }


        #region Used when running tests without Testcontainer
        //private void EnsureDatabase()
        //{
        //    using var scope = _scopeFactory.CreateScope();

        //    var context = scope.ServiceProvider.GetService<OrderContext>();

        //    context.Database.Migrate();
        //}

        //public async Task ResetDbState()
        //{
        //    await _checkpoint.Reset(_configuration.GetConnectionString("OrderingConnectionString"));
        //}

        //public void Dispose()
        //{
        //    using var scope = _scopeFactory.CreateScope();

        //    var context = scope.ServiceProvider.GetService<OrderContext>();

        //    context.Dispose();
        //}

        #endregion
    }
}
