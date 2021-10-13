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
                Predicate = _ => true, // this way we say that we don't need any additional health checks like DbContext for this endpoint
                ResponseWriter = HealthCheckResponses.WriteJsonResponse
            });

            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponses.WriteJsonResponse
            });

            return endpoints;
        }
    }
}
