using Grpc.Health.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shopping.HealthChecks;
using Shopping.WebStatus.Grpc.HealthChecks;
using Shopping.WebStatus.Grpc.Services;
using System;

namespace Shopping.WebStatus
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
            services
                .AddHealthChecks()
                .AddCheck<DiscountServiceHealthCheck>("Discount Service", tags: new string[] { "discount service ready", "grpc" })
                .AddCheck<DiscountDbServiceHealthCheck>("Discount Db", tags: new string[] { "discount db ready", "postgresql" })
                .Services
                .AddHealthChecksUI(setup =>
                {
                    // we are passing custom HealthCheck /healthcheck which will be called by JS (Ajax) end added to UI
                    // with response from our custom checks (DiscountServiceHealthCheck, ...) 
                    setup.AddHealthCheckEndpoint("Discount gRpc Services Health Check", "/healthcheck");
                })
                //.AddSqlServerStorage("server=localhost;initial catalog=healthchecksui;user id=sa;password=Password12!");
                .AddInMemoryStorage();

            services.AddScoped<DiscountServiceStatus>();
            services.AddScoped<DiscountDbServiceStatus>();
            services.AddGrpcClient<Health.HealthClient>(config =>
            { 
                config.Address = new Uri(Configuration["GrpcSettings:DiscountHealthCheckUrl"]);
            });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapHealthChecksUI(options =>
                {
                    //options.UIPath = "/healthchecks-ui"; default
                    options.ApiPath = "/health-ui-api";
                });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
