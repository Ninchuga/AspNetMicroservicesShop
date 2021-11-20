using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Net.Http;

namespace Shopping.Policies.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddWrappedPolicyHandlers(this IHttpClientBuilder httpClientBuilder, params IAsyncPolicy<HttpResponseMessage>[] policies)
        {
            httpClientBuilder.AddPolicyHandler(Policy.WrapAsync(policies));

            return httpClientBuilder;
        }
    }
}
