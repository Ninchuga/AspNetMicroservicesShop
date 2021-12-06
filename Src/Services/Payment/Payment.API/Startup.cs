using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Payment.API.Consumers;
using System;

namespace Payment.API
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
                    config.AddConsumer<BillOrderConsumer>();
                    config.AddConsumer<RollbackOrderPaymentConsumer>();

                    config.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(Configuration.GetConnectionString("AzureServiceBusConnectionString"));

                        cfg.Send<OrderBilled>(s => s.UseSessionIdFormatter(c => c.Message.CorrelationId.ToString()));

                        cfg.ReceiveEndpoint(EventBusConstants.OrderBillingQueue, endpoint =>
                        {
                            endpoint.ConfigureConsumer<BillOrderConsumer>(context);
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Interval(3, TimeSpan.FromMilliseconds(200));
                                r.Ignore<ArgumentNullException>();
                                r.Handle<InvalidOperationException>();
                            });

                            // use the outbox to prevent duplicate events from being published
                            endpoint.UseInMemoryOutbox();
                        });

                        cfg.ReceiveEndpoint(EventBusConstants.OrderBillingRollbackQueue, endpoint =>
                        {
                            endpoint.ConfigureConsumer<RollbackOrderPaymentConsumer>(context);
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
            else
            {
                services.AddMassTransit(config =>
                {
                    config.AddConsumer<BillOrderConsumer>();
                    config.AddConsumer<RollbackOrderPaymentConsumer>();

                    config.UsingRabbitMq((ctx, cfg) =>
                    {
                        cfg.Host(Configuration["EventBusSettings:HostAddress"]);

                        cfg.ReceiveEndpoint(EventBusConstants.OrderBillingQueue, endpoint =>
                        {
                            endpoint.ConfigureConsumer<BillOrderConsumer>(ctx);
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Interval(3, TimeSpan.FromMilliseconds(200));
                                r.Ignore<ArgumentNullException>();
                                r.Handle<InvalidOperationException>();
                            });
                        });

                        cfg.ReceiveEndpoint(EventBusConstants.OrderBillingRollbackQueue, endpoint =>
                        {
                            endpoint.ConfigureConsumer<RollbackOrderPaymentConsumer>(ctx);
                            endpoint.UseMessageRetry(r =>
                            {
                                r.Interval(3, TimeSpan.FromMilliseconds(200));
                                r.Ignore<ArgumentNullException>();
                                r.Handle<InvalidOperationException>();
                            });
                        });
                    });
                });
            }
            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentApi v1"));
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
