using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.MVC.Extensions
{
    /// <summary>
    /// Used as global exception handler
    /// </summary>
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
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                _logger.LogError(error.Message);
                _logger.LogError(error.InnerException?.Message);
                _logger.LogError($"Unhandled exception occurred. Source of exception: {error.Source}");
                _logger.LogError(error.StackTrace);

                //switch (error)
                //{
                //    //case AppException e:
                //    //    // custom application error
                //    //    response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    //    break;
                //    case KeyNotFoundException e:
                //        // not found error
                //        response.StatusCode = (int)HttpStatusCode.NotFound;
                //        break;
                //    default:
                //        // unhandled error
                //        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //        break;
                //}

                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
