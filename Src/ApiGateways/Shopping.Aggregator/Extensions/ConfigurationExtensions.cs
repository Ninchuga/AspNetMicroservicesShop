using Microsoft.AspNetCore.Builder;
using Shopping.Correlation;
using Shopping.Logging;

namespace Shopping.Aggregator.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static void AddCorrelationIdMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
        }

        public static void AddCorrelationLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdLoggingMiddleware>();
        }
    }
}
