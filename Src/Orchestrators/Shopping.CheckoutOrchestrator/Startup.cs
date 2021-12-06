using EventBus.Messages.Common;
using MassTransit;
using MassTransit.Saga;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shopping.OrderSagaOrchestrator.Consumers;
using Shopping.OrderSagaOrchestrator.Extensions;
using Shopping.OrderSagaOrchestrator.Persistence;
using Shopping.OrderSagaOrchestrator.StateMachine;
using System.Reflection;

namespace Shopping.OrderSagaOrchestrator
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
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();
            services.ConfigureMassTransitWithRabbitMq(Configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Saga", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderSaga v1"));
            }

            app.UseRouting();

            //app.UseAuthorization();

            if(Configuration.GetValue<bool>("UseAzureServiceBus"))
            {
                var bus = app.ApplicationServices.GetService<IServiceBusConsumer>();
                bus.RegisterOnMessageHandlerAndReceiveMessages().GetAwaiter().GetResult();

                //var busSubscription = app.ApplicationServices.GetService<IServiceBusTopicSubscription>();
                //busSubscription.PrepareFiltersAndHandleMessages().GetAwaiter().GetResult();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
