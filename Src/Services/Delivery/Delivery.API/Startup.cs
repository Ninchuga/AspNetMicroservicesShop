using Delivery.API.Consumers;
using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery.API
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
            services.AddControllers();

            bool useAzureServiceBus = Configuration.GetValue<bool>("UseAzureServiceBus");
            if (useAzureServiceBus)
            {
                services.AddMassTransit(config =>
                {
                    config.AddConsumer<DispatchOrderConsumer>();

                    config.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(Configuration.GetConnectionString("AzureServiceBusConnectionString"));

                        cfg.Send<OrderDelivered>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

                        cfg.ReceiveEndpoint(EventBusConstants.OrderDeliveryQueue, endpoint =>
                        {
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Ignore<ArgumentNullException>();
                                r.Interval(3, TimeSpan.FromSeconds(5));
                            });
                            endpoint.ConfigureConsumer<DispatchOrderConsumer>(context);

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });
                    });
                });
            }
            else
            {
                services.AddMassTransit(config =>
                {
                    config.AddConsumer<DispatchOrderConsumer>();

                    config.UsingRabbitMq((ctx, config) =>
                    {
                        config.Host(Configuration["EventBusSettings:HostAddress"]);

                        config.ReceiveEndpoint(EventBusConstants.OrderDeliveryQueue, endpoint =>
                        {
                            endpoint.ConfigureConsumer<DispatchOrderConsumer>(ctx);
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Interval(3, TimeSpan.FromMilliseconds(200));
                                r.Ignore<ArgumentNullException>();
                                r.Handle<InvalidOperationException>();
                            });

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });
                    });
                });
            }
            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Delivery API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Delivery Api v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
