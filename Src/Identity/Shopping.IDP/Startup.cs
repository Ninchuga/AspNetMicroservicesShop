// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shopping.IDP.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Shopping.IDP.Persistence;
using Shopping.IDP.Models;
using Microsoft.AspNetCore.Identity;
using System;
using Shopping.IDP.Certificates;
using IdentityServer4.Services;
using Shopping.HealthChecks;

namespace Shopping.IDP
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("IdentityDb");

            services.AddHealthChecks()
                .AddSqlServer(connectionString, name: "Identity Db", tags: new string[] { "identity db ready", "sql server" });

            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("default", policy =>
            //    {
            //        policy.WithOrigins(Configuration["WebClientUrls:Angular"])
            //            .AllowAnyHeader()
            //            .AllowAnyMethod()
            //            .AllowCredentials();
            //    });
            //});

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddIdentityServer(x => x.IssuerUri = Configuration["IdentityIssuer"])
                //.AddSigningCredential(Certificate.Get()) // use for production and store certificate in Azure Key Vault or some other certificate storage
                .AddDeveloperSigningCredential() // use just for test purposes
                //.AddInMemoryApiResources(Config.ApiResources)
                //.AddInMemoryApiScopes(Config.ApiScopes)
                //.AddInMemoryClients(Config.Clients(Configuration))
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                //.AddTestUsers(Config.GetUsers())
                .AddAspNetIdentity<ApplicationUser>()
                 // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    //options.TokenCleanupInterval = 30;
                })
                //.AddCorsPolicyService<InMemoryCorsPolicyService>()
                .AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors("default");
            //app.UseCors(builder => builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

            //app.UseCsp(opts => opts
            //    .BlockAllMixedContent()
            //    .ScriptSources(s => s.Self()).ScriptSources(s => s.UnsafeEval())
            //    .StyleSources(s => s.UnsafeInline())
            //);

            // enable to test w/ CSP
            //app.Use(async (ctx, next) =>
            //{
            //    ctx.Response.OnStarting(() =>
            //    {
            //        if (ctx.Response.ContentType?.StartsWith("text/html") == true)
            //        {
            //            ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src http://localhost:5000 http://localhost:3721; frame-src 'self' http://localhost:5000");
            //        }
            //        return Task.CompletedTask;
            //    });

            //    await next();
            //});


            app.UseStaticFiles();

            app.UseForwardedHeaders();
            app.UseIdentityServer();

            // Fix a problem with chrome. Chrome enabled a new feature "Cookies without SameSite must be secure", 
            // the coockies shold be expided from https, but in eShop, the internal comunicacion in aks and docker compose is http.
            // To avoid this problem, the policy of cookies shold be in Lax mode.
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });
            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
