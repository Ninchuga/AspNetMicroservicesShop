using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ordering.API.Extensions;
using Ordering.API.Filters;
using Ordering.Application;
using Ordering.Application.HubConfiguration;
using Ordering.Infrastructure;
using Shopping.HealthChecks;
using System;
using System.Collections.Generic;

namespace Ordering.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Used for storing access tokens in the cache in a delegating handlers
            services.AddAccessTokenManagement();
            services.AddHttpContextAccessor();
            services.AddApplicationServices(Configuration);
            services.AddInfrastructureServices(Configuration);

            // Add Cors policy because of the SignalR client
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(Configuration["RazorWebClientUrl"])
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                options.Audience = "orderapi";
                options.RequireHttpsMetadata = false;
            });

            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering.API", Version = "v1" });

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
                                { "orderapi.fullaccess", "Ordering API" }
                            }
                        }
                    }
                });

                c.OperationFilter<AuthorizeCheckOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering.API v1");
                    c.OAuthClientId("orderingswaggerui");
                    c.OAuthAppName("Order Swagger UI");
                });
            }

            app.AddCorrelationIdMiddleware();
            app.AddCorrelationLoggingMiddleware();

            app.UseHttpsRedirection();
            app.UseRouting();

            // Add Cors policy because of the SignalR client
            app.UseCors("CorsPolicy");

            if (Configuration.GetValue<bool>("Azure:UseAzureSignalR"))
                app.UseFileServer();
            else
                app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapControllers().RequireAuthorization();
                endpoints.MapHub<OrderStatusHub>("/orderstatushub");
            });
        }
    }
}
