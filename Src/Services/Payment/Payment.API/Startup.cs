using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Payment.API.Extensions;
using Payment.API.Filters;
using Payment.API.Services;
using Shopping.HealthChecks;
using System;
using System.Collections.Generic;

namespace Payment.API
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
            services.AddControllers();
            // Used for storing access tokens in the cache in a delegating handlers
            services.AddAccessTokenManagement();

            services.AddHttpClient<ITokenValidationService, TokenValidationService>();
            services.AddHttpClient<ITokenExchangeService, TokenExchangeService>();

            bool useAzureServiceBus = Configuration.GetValue<bool>("UseAzureServiceBus");
            if (useAzureServiceBus)
                services.ConfigureMassTransitWithAzureServiceBus(Configuration);
            else
                services.ConfigureMassTransitWithRabbitMQ(Configuration);

            services.AddHealthChecks()
                .AddRabbitMQ(Configuration["EventBusSettings:HostAddress"], null, "Rabbit MQ", HealthStatus.Degraded, tags: new string[] { "rabbit ready" }, TimeSpan.FromSeconds(5));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                 options.Audience = "paymentapi";
                 options.RequireHttpsMetadata = false;
             });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment API", Version = "v1" });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/authorize"),
                            TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/token"),
                            Scopes = new Dictionary<string, string>()
                            {
                                { "paymentapi.fullaccess", "Payment API" }
                            }
                        }
                    }
                });

                c.OperationFilter<AuthorizeCheckOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment.API v1");
                    c.OAuthClientId("paymentswaggerui");
                    c.OAuthAppName("Payment Swagger UI");
                });
            }

            app.UseHttpsRedirection();
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
