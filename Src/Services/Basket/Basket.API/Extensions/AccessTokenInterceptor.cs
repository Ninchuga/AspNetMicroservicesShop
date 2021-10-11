using Basket.API.Constants;
using Basket.API.Factories;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Basket.API.Extensions
{
    public class AccessTokenInterceptor : Interceptor
    {
        private readonly ITokenExchangeServiceFactory _tokenExchangeServiceFactory;

        public AccessTokenInterceptor(ITokenExchangeServiceFactory tokenExchangeServiceFactory)
        {
            _tokenExchangeServiceFactory = tokenExchangeServiceFactory;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = new Metadata();
            var tokenExchangeService = _tokenExchangeServiceFactory.GetTokenExchangeServiceInstance(DownstreamServices.DiscountApi);
            var token = tokenExchangeService.GetAccessTokenForDownstreamService().GetAwaiter().GetResult();

            headers.Add(new Metadata.Entry("Authorization", $"Bearer {token}"));
            var newOptions = context.Options.WithHeaders(headers);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

            return base.AsyncUnaryCall(request, newContext, continuation);
        }
    }
}
