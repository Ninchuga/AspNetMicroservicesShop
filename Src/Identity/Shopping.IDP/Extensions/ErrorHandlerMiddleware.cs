using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.IDP.Extensions
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                _logger.LogInformation("Executing request path {RequestPath}", context.Request.Path);
                _logger.LogInformation("Executing request path {RequestHost}", context.Request.Host);
                _logger.LogInformation("Executing request path {RequestQueryString}", context.Request.QueryString);

                await _next(context);

                _logger.LogInformation("Request successfully executed");
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                _logger.LogError(error.Message);
                _logger.LogError(error.InnerException?.Message);
                _logger.LogError($"Unhandled exception occurred. Source of exception: {error.Source}");
                _logger.LogError(error.StackTrace);

                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
