using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.MVC.Extensions
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
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                _logger.LogError(error.Message);
                _logger.LogError(error.InnerException?.Message);
                _logger.LogError($"Unhandled exception occurred. Source of exception: {error.Source}");
                _logger.LogError($"HTTP method: {context.Request.Method}");
                _logger.LogError($"Request path: {context.Request.Path}");
                _logger.LogError($"Is request HTTPS: {context.Request.IsHttps}");
                _logger.LogError($"Request protocol: {context.Request.Protocol}");
                _logger.LogError($"Request scheme: {context.Request.Scheme}");
                _logger.LogError($"Remote IP Address: {context.Connection.RemoteIpAddress}");
                _logger.LogError($"Remote port: {context.Connection.RemotePort}");
                _logger.LogError($"Local IP Address: {context.Connection.LocalIpAddress}");
                _logger.LogError($"Local port: {context.Connection.LocalPort}");
                _logger.LogError($"Client certificate: {context.Connection.ClientCertificate?.RawData}");
                _logger.LogError($"Client certificate subject: {context.Connection.ClientCertificate?.Subject}");
                //_logger.LogError($"Client certificate subject: {await context.Connection.GetClientCertificateAsync()}");
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
