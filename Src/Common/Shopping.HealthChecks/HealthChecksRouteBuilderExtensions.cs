using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.HealthChecks
{
    public static class HealthChecksRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapDefaultHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => true, // runs all mapped helathchekcs e.g. dbContext
                ResponseWriter = HealthCheckResponses.WriteJsonResponse
            })
            .WithMetadata(new AllowAnonymousAttribute());

            endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions
            {
                Predicate = _ => true, // runs all mapped helathchekcs e.g. DbContext, Redis, RabbitMq ...
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            })
            .WithMetadata(new AllowAnonymousAttribute());

            return endpoints;
        }
    }
}
