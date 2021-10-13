using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Shopping.HealthChecks
{
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
