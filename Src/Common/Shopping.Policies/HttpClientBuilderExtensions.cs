using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Shopping.Policies
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddWrappedPoliciesHandlers(this IHttpClientBuilder httpClientBuilder, IServiceCollection services, params IAsyncPolicy<HttpResponseMessage>[] policies)
        {
            var serviceProvider = services.BuildServiceProvider();
            var policyHolder = serviceProvider.GetRequiredService<IPolicyHolder>();

            httpClientBuilder.AddPolicyHandler(policyHolder.WrapPolicies(policies));

            return httpClientBuilder;
        }
    }
}
