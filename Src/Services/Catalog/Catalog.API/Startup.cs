using Catalog.API.Data;
using Catalog.API.Extensions;
using Catalog.API.Filters;
using Catalog.API.Repositories;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shopping.HealthChecks;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Catalog.API
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
            services.AddHttpContextAccessor();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                options.Audience = "catalogapi";
                options.RequireHttpsMetadata = false; // added because of a healthcheck
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanRead", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
                options.AddPolicy("HasFullAccess", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));
            });

            services.AddScoped<ICatalogContext, CatalogContext>();
            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddControllers();

            services.AddHealthChecks()
                .AddMongoDb(Configuration["DatabaseSettings:ConnectionString"], "Catalog Db", HealthStatus.Degraded, tags: new string[] { "catalog db ready", "mongo" }, TimeSpan.FromSeconds(2));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });

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
                                { "catalogapi.fullaccess", "Catalog API" }
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1");
                    c.OAuthClientId("catalogswaggerui");
                    c.OAuthAppName("Catalog Swagger UI");
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
