using Microsoft.AspNetCore.Builder;
using Shopping.Logging;

namespace Shopping.Aggregator.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static void AddCorrelationLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdLoggingMiddleware>();
        }
    }
}
