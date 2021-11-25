using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shopping.Correlation.Constants;
using System;
using System.Threading.Tasks;

namespace Shopping.Correlation
{
    /// <summary>
    /// Used in Client apps to generate correlation id for each request to downstream services
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
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
            }

            context.Items.Add(Headers.CorrelationIdHeader, correlationId);

            await _next(context);
        }
    }
}
