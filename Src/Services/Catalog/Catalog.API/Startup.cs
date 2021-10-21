using Catalog.API.Data;
using Catalog.API.Extensions;
using Catalog.API.Repositories;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
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
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanRead", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
                options.AddPolicy("HasFullAccess", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));
            });

            services.AddScoped<ICatalogContext, CatalogContext>();
            services.AddScoped<IProductRepository, ProductRepository>();

            // Apply to all controllers authorization policy which requires all users to be authorized before executing actions
            var requiredAuthenticatedUserPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            services.AddControllers(configure =>
            {
                configure.Filters.Add(new AuthorizeFilter(requiredAuthenticatedUserPolicy));
            });

            services.AddHealthChecks()
                .AddMongoDb(Configuration["DatabaseSettings:ConnectionString"], "Catalog Db", HealthStatus.Degraded, tags: new string[] { "catalog db ready", "mongo" }, TimeSpan.FromSeconds(2));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
            }

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
    }
}
