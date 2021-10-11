using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shopping.Common.Constants;
using System;
using System.Threading.Tasks;

namespace Shopping.Common.Correlations
{
    /// <summary>
    /// Used in Client apps to generate correlation id for each request to downstream services
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string correlationId;
            if(context.Request.Headers.ContainsKey(Headers.CorrelationIdHeader))
            {
                correlationId = context.Request.Headers[Headers.CorrelationIdHeader][0];
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
                context.Items.Add(Headers.CorrelationIdHeader, correlationId);
            }

            // maybe there are log messages that are used before calling downstream apis
            // place them inside the same correlation scope as downstream services
            using (_logger.BeginScope("{CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }
}
