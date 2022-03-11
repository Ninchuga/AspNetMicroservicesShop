using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ordering.API;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Services;

namespace Shopping.IntegrationTests.Utility.Ordering
{
    public class OrderingTestStartup : Startup
    {
        public OrderingTestStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.Configure(app, env);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            AddTestServices(services);
        }

        private void AddTestServices(IServiceCollection services)
        {
            services.AddTransient(provider => Mock.Of<IEmailService>());
            services.AddTransient(provider => Mock.Of<IPublishEndpoint>());
            services.AddTransient(provider => Mock.Of<ITokenExchangeService>());
        }
    }
}
