// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shopping.IDP.Extensions;
using Shopping.IDP.Persistence;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Shopping.IDP.Models;
using Shopping.Logging;
using Microsoft.Extensions.Configuration;

namespace Shopping.IDP
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args)
                .Build()
                .MigrateDatabase<PersistedGrantDbContext>((context, service) =>
                {
                    // there is no seed for this context
                })
                .MigrateDatabase<ConfigurationDbContext>((context, service) =>
                {
                    var configuration = service.GetRequiredService<IConfiguration>();
                    IdentityDbSeed.SeedIdentityConfiguration(context, Log.Logger, configuration).Wait();
                })
                .MigrateDatabase<ApplicationDbContext>((context, service) =>
                {
                    var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

                    IdentityDbSeed.SeedIdentityUsers(context, Log.Logger, userManager, roleManager).Wait();
                })
                .Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(LoggingConfiguration.Configure)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
                
    }
}