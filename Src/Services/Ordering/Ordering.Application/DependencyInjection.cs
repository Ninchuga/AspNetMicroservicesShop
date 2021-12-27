using EventBus.Messages.Common;
using FluentValidation;
using GreenPipes;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Behaviours;
using Ordering.Application.EventBusConsumers;
using Ordering.Application.Services;
using Shopping.Correlation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ordering.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddHttpClient<ITokenValidationService, TokenValidationService>();

            services.AddHttpClient("OrderSaga", config =>
            {
                config.BaseAddress = new Uri(configuration["OrderSagaUrl"]);
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            // There is no predefined transport for SignalR. It will try to find by itself the best suited transport
            // 1. WebSocket if newer versions of browser are used on the client
            // 2. Server Sent Events (SSE) if WebSockets are not available
            // 3. Long Polling
            // Objects will be serialized by default JSON Hub Protocol in the background and sent to client
            // MessagePack protocol for binary format is not included in default SignalR nuget package. To include it add package Microsoft.AspNetCore.SignalR.Protocols.MessagePack
            if (configuration.GetValue<bool>("UseAzureSignalR"))
                services.AddSignalR().AddAzureSignalR(configuration.GetConnectionString("AzureSignalRConnectionString"));
            else
                services.AddSignalR();

            return services;
        }
    }
}
