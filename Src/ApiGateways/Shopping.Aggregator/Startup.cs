using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.DelegatingHandlers;
using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Services;
using Shopping.Correlation;
using Shopping.HealthChecks;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Shopping.Aggregator
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
            services.AddHealthChecks();
            services.AddHttpContextAccessor();

            // Used for storing access tokens in the cache in a delegating handlers
            services.AddAccessTokenManagement();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                 options.Audience = "shoppingaggregator";
                 options.RequireHttpsMetadata = false;
             });

            services.AddTransient<CatalogApiTokenExchangeDelegatingHandler>();
            services.AddTransient<BasketApiTokenExchangeDelegatingHandler>();
            services.AddTransient<OrderApiTokenExchangeDelegatingHandler>();
            services.AddTransient<CorrelationIdDelegatingHandler>();

            services.AddHttpClient<ICatalogService, CatalogService>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiSettings:Catalog:CatalogUrl"]))
                .AddHttpMessageHandler<CatalogApiTokenExchangeDelegatingHandler>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddHttpClient<IBasketService, BasketService>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiSettings:Basket:BasketUrl"]))
                .AddHttpMessageHandler<BasketApiTokenExchangeDelegatingHandler>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddHttpClient<IOrderService, OrderService>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiSettings:Ordering:OrderingUrl"]))
                .AddHttpMessageHandler<OrderApiTokenExchangeDelegatingHandler>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping.Aggregator", Version = "v1" });
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping.Aggregator v1"));
            }

            app.AddCorrelationLoggingMiddleware();

            //app.UseHttpsRedirection();
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
