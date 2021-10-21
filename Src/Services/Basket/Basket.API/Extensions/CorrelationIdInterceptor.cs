using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Shopping.Correlation.Constants;
using System;
using System.Diagnostics;
using System.Linq;

namespace Basket.API.Extensions
{
    public class CorrelationIdInterceptor : Interceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            string correlationId = string.Empty;
            if(_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(Headers.CorrelationIdHeader))
            {
                correlationId = _httpContextAccessor.HttpContext.Request.Headers[Headers.CorrelationIdHeader][0];
            }

            context.Options.Headers.Add(Headers.CorrelationIdHeader, correlationId);

            return base.AsyncUnaryCall(request, context, continuation);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var header = context.Options.Headers.FirstOrDefault(x => x.Key == Headers.CorrelationIdHeader);
            Trace.CorrelationManager.ActivityId = header != null ? Guid.Parse(header.Value) : Guid.Empty;
            return base.AsyncServerStreamingCall(request, context, continuation);
        }
    }
}
