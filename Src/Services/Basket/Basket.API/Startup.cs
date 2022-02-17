using Basket.API.Extensions;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Basket.API.Services.Basket;
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
                options.Configuration = Configuration.GetValue<string>("CacheSettings:ConnectionString");
            });

            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<DiscountGrpcService>();
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
            services.AddMassTransitHostedService();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"bearer {token}\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1"));
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
