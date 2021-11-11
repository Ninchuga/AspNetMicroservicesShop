using Grpc.Core;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Basket.API.Policies
{
    public class GrpcPolicies
    {
        private static ILogger<GrpcPolicies> _logger;

        public GrpcPolicies(ILogger<GrpcPolicies> logger)
        {
            _logger = logger;
        }

        private static HttpStatusCode[] serverErrors = new HttpStatusCode[] {
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.TooManyRequests,
                HttpStatusCode.RequestTimeout
            };

        private static StatusCode[] gRpcErrors = new StatusCode[] {
                StatusCode.DeadlineExceeded,
                StatusCode.Internal,
                StatusCode.NotFound,
                StatusCode.ResourceExhausted,
                StatusCode.Unavailable,
                StatusCode.Unknown
            };

        public static Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> Retry = (request) =>
        {
            return Policy.HandleResult<HttpResponseMessage>(r => 
            {

                var grpcStatus = StatusManager.GetStatusCode(r);
                var httpStatusCode = r.StatusCode;

                return (grpcStatus == null && serverErrors.Contains(httpStatusCode)) || // if the server send an error before gRPC pipeline
                       (httpStatusCode == HttpStatusCode.OK && gRpcErrors.Contains(grpcStatus.Value)); // if gRPC pipeline handled the request (gRPC always answers OK)
            })
            .WaitAndRetryAsync(3, (input) => TimeSpan.FromSeconds(3 + input), (result, timeSpan, retryCount, context) =>
            {
                var grpcStatus = StatusManager.GetStatusCode(result.Result);
                _logger.LogWarning($"Request failed with {grpcStatus}. Retry");
            });
        };

        public static Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> CircuitBreaker = (request) =>
        {
            return Policy.HandleResult<HttpResponseMessage>(r => 
            {

                var grpcStatus = StatusManager.GetStatusCode(r);
                var httpStatusCode = r.StatusCode;

                return (grpcStatus == null && serverErrors.Contains(httpStatusCode)) || // if the server send an error before gRPC pipeline
                       (httpStatusCode == HttpStatusCode.OK && gRpcErrors.Contains(grpcStatus.Value)); // if gRPC pipeline handled the request (gRPC always answers OK)
            })
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(15));
        };
    }

    public static class StatusManager
    {
        public static StatusCode? GetStatusCode(HttpResponseMessage response)
        {
            var headers = response.Headers;

            if (!headers.Contains("grpc-status") && response.StatusCode == HttpStatusCode.OK)
                return StatusCode.OK;

            if (headers.Contains("grpc-status"))
                return (StatusCode)int.Parse(headers.GetValues("grpc-status").First());

            return null;
        }
    }
}
