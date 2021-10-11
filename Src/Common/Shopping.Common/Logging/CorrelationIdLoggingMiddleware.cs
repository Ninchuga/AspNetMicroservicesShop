using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Shopping.Common.Logging
{
    /// <summary>
    /// Used in downstream services for logging scope with correlation id
    /// </summary>
    public class CorrelationIdLoggingMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdLoggingMiddleware> _logger;

        public CorrelationIdLoggingMiddleware(RequestDelegate next, ILogger<CorrelationIdLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey(CorrelationIdHeader))
            {
                string correlationId = context.Request.Headers[CorrelationIdHeader][0];
                using (_logger.BeginScope("{CorrelationId}", correlationId))
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
