using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Shopping.HealthChecks
{
    /// <summary>
    /// Checks Health Status of the specified downstream service
    /// </summary>
    public static class DownstreamApiHealthCheckExtensions
    {
        public static IHealthChecksBuilder AddApiHealth(this IHealthChecksBuilder healthChecksBuilder,
            Uri apiEndpoint, string apiName = "ApiHealth", HealthStatus failureStatus = HealthStatus.Degraded, 
            IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return healthChecksBuilder.AddCheck(apiName, new DownstreamApiHealthCheck(apiEndpoint), failureStatus, tags, timeout);
        }
    }
}
