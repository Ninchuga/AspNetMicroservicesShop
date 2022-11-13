using Basket.API.Constants;
using Basket.API.Extensions;
using Basket.API.Filters;
using Basket.API.Repositories;
using Basket.API.Services.Basket;
using Basket.API.Services.Discount;
using Basket.API.Services.Tokens;
using Discount.Grpc.Protos;
using IdentityServer4.AccessTokenValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shopping.HealthChecks;
using Shopping.Policies.Grpc;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Basket.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // clear Microsoft changed claim names from dictionary and preserve original ones
            // e.g. Microsoft stack renames the 'sub' claim name to http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddRedis(Configuration["CacheSettings:ConnectionString"], "Basket Db", HealthStatus.Degraded, tags: new string[] { "basket db ready", "redis" }, TimeSpan.FromSeconds(5))
                .AddRabbitMQ(Configuration["EventBusSettings:HostAddress"], null, "Rabbit MQ", HealthStatus.Degraded, tags: new string[] { "rabbit ready" }, TimeSpan.FromSeconds(5));

            services.AddHttpContextAccessor();
            services.AddAccessTokenManagement(); // for token cache

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                 options.Audience = "basketapi";
                 options.RequireHttpsMetadata = false;
             });

            // Redis configuration
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("CacheSettings:RedisInstanceName");
                options.ConfigurationOptions = new ConfigurationOptions()
                {
                    EndPoints = { Configuration.GetValue<string>("CacheSettings:RedisConnectionString") },
                    ConnectRetry = 10,
                    ReconnectRetryPolicy = new LinearRetry(1500),
                    ConnectTimeout = 5000
                };
            });

            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IDiscountService, DiscountGrpcService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<AccessTokenInterceptor>();
            services.AddTransient<CorrelationIdInterceptor>();
            services.AddTransient<ITokenExchangeService, TokenExchangeService>();

            services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o =>
            {
                o.Address = new Uri(Configuration["GrpcSettings:DiscountUrl"]);
            })
            .AddInterceptor<AccessTokenInterceptor>()
            .AddInterceptor<CorrelationIdInterceptor>()
            .AddPolicyHandler(GrpcPolicies.Retry)
            .AddPolicyHandler(GrpcPolicies.CircuitBreaker);

            // MassTransit-RabbitMQ Configuration
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((ctx, config) =>
                {
                    config.Host(Configuration["EventBusSettings:HostAddress"]);
                    //config.UseSerilog();
                    //config.UseSerilogMessagePropertiesEnricher();
                });
            });
            if (Environment.GetEnvironmentVariable(BasketApiEnvironments.AspNetCoreEnvironmentVariable) != BasketApiEnvironments.TestingEnvironment)
            {
                // Used to start the bus
                services.AddMassTransitHostedService();
            }

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });

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
                                { "basketapi.fullaccess", "Basket API" }
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1");
                    c.OAuthClientId("basketswaggerui");
                    c.OAuthAppName("Basket Swagger UI");
                });
            }

            app.AddCorrelationLoggingMiddleware();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}
