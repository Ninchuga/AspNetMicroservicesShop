using Basket.API.Services.Tokens;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;

namespace Basket.API.Extensions
{
    public class AccessTokenInterceptor : Interceptor
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenExchangeService _tokenExchangeService;

        public AccessTokenInterceptor(IConfiguration configuration, ITokenExchangeService tokenExchangeService)
        {
            _configuration = configuration;
            _tokenExchangeService = tokenExchangeService;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = new Metadata();
            string downstreamApiAccessTokenCacheKey = _configuration.GetValue<string>("DownstreamServicesTokenExhangeCacheKeys:DiscountApi");
            string downstreamApiScopes = _configuration.GetValue<string>("DownstreamServicesScopes:DiscountApi");
            var token = _tokenExchangeService.GetAccessTokenForDownstreamService(downstreamApiAccessTokenCacheKey, downstreamApiScopes).GetAwaiter().GetResult();

            headers.Add(new Metadata.Entry("Authorization", $"Bearer {token}"));
            var newOptions = context.Options.WithHeaders(headers);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

            return base.AsyncUnaryCall(request, newContext, continuation);
        }
    }
}
