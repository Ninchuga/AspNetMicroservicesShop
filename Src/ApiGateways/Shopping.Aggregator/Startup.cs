using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Registry;
using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.DelegatingHandlers;
using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Filters;
using Shopping.Aggregator.Services;
using Shopping.Correlation;
using Shopping.HealthChecks;
using Shopping.Logging;
using Shopping.Policies;
using Shopping.Policies.Extensions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

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
            services.AddMemoryCache();

            // Used for storing access tokens in the cache in a delegating handlers
            services.AddAccessTokenManagement();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                 options.Audience = "shoppingaggregator";
                 options.RequireHttpsMetadata = false;
             });

            services.AddTransient<TokenExchangeDelegatingHandler>();
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddTransient<LoggingDelegatingHandler>();

            IPolicyRegistry<string> registry = services.AddPolicyRegistry()
                .RegisterPolicies(services,
                    new InMemoryCachePolicy(ttl: TimeSpan.FromMinutes(5)),
                    new FallbackPolicy(),
                    new RetryPolicy(retryCount: 3),
                    new CircuitBreakerPolicy(allowedNumberOfAttemptsBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(10)),
                    new TimeoutPolicy(secondsToWaitForResponse: 10));

            services.AddHttpClient<ICatalogService, CatalogService>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiSettings:Catalog:CatalogUrl"]))
                .AddHttpMessageHandler<TokenExchangeDelegatingHandler>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandlerFromRegistry(AvailablePolicies.FallbackPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.RetryPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.CircuitBreakerPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.TimeoutPolicy.ToString());

            services.AddHttpClient<IBasketService, BasketService>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiSettings:Basket:BasketUrl"]))
                .AddHttpMessageHandler<TokenExchangeDelegatingHandler>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandlerFromRegistry(AvailablePolicies.FallbackPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.RetryPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.CircuitBreakerPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.TimeoutPolicy.ToString());

            services.AddHttpClient<IOrderService, OrderService>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiSettings:Ordering:OrderingUrl"]))
                .AddHttpMessageHandler<TokenExchangeDelegatingHandler>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandlerFromRegistry(AvailablePolicies.FallbackPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.RetryPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.CircuitBreakerPolicy.ToString())
                .AddPolicyHandlerFromRegistry(AvailablePolicies.TimeoutPolicy.ToString());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping.Aggregator", Version = "v1" });

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
                                { "shoppingaggregator.fullaccess", "Shopping Aggregator API" }
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping.Aggregator v1");
                    c.OAuthClientId("shoppingaggregatorswaggerui");
                    c.OAuthAppName("Shopping Aggregator Swagger UI");
                });
            }

            app.AddCorrelationIdMiddleware();
            app.AddCorrelationLoggingMiddleware();

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

        private IAsyncPolicy<HttpResponseMessage> PolicySelector(IReadOnlyPolicyRegistry<string> policyRegistry, HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Method == HttpMethod.Get)
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");
            }
            // do not use retry on POST methods because they are not idempotent 
            // multiple requests will be sent and all of them could be executed resulting in duplcated or broken data
            // potentially could cause exceptions
            else if (httpRequestMessage.Method == HttpMethod.Post) 
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy");
            }
            else
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleWaitAndRetryPolicy");
            }
        }
    }
}
