using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shopping.HealthChecks;
using Shopping.OrderSagaOrchestrator.Extensions;
using Shopping.OrderSagaOrchestrator.Services;

namespace Shopping.OrderSagaOrchestrator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();

            // Used for storing access tokens in the cache
            services.AddAccessTokenManagement();

            services.AddHttpClient<ITokenValidationService, TokenValidationService>();
            services.AddHttpClient<ITokenExchangeService, TokenExchangeService>();

            bool useAzureServiceBus = Configuration.GetValue<bool>("UseAzureServiceBus");
            if (useAzureServiceBus)
                services.ConfigureMassTransitWithAzureServiceBus(Configuration);
            else
            {
                services.ConfigureDatabase(Configuration)
                    .ConfigureMassTransitWithRabbitMQ(Configuration);
            }

            services.AddOrderSagaOrchestratorHealthChecks(Configuration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                 options.Audience = "ordersagaorchestrator";
                 options.RequireHttpsMetadata = false;
             });

            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderSagaOrchestrator v1"));
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapControllers();
            });
        }
    }
}
