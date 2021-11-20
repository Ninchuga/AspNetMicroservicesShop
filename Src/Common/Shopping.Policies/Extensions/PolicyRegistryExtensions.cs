using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using System;

namespace Shopping.Policies.Extensions
{
    public static class PolicyRegistryExtensions
    {
        /// <summary>
        /// Register all available policies on application startup
        /// </summary>
        /// <param name="registry">Registry for policies</param>
        /// <param name="services">Service collection for resolving dependencies</param>
        /// <param name="policies">Policies to add to registry</param>
        /// <returns>Registry with added policies</returns>
        public static IPolicyRegistry<string> RegisterPolicies(this IPolicyRegistry<string> registry, IServiceCollection services, params IAmPolicy[] policies)
        {
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

            foreach (var policy in policies)
            {
                switch (policy.PolicyName)
                {
                    case nameof(AvailablePolicies.FallbackPolicy):
                        var fallback = policy as FallbackPolicy;
                        var fallbackLogger = loggerFactory.CreateLogger<FallbackPolicy>();
                        registry.Add(fallback.PolicyName, fallback.FallbackPolicyHandler(fallbackLogger));
                        break;
                    case nameof(AvailablePolicies.RetryPolicy):
                        var retry = policy as RetryPolicy;
                        var retryLogger = loggerFactory.CreateLogger<RetryPolicy>();
                        registry.Add(retry.PolicyName, retry.RetryPolicyHandler(retryLogger));
                        break;
                    case nameof(AvailablePolicies.CircuitBreakerPolicy):
                        var circuitBreaker = policy as CircuitBreakerPolicy;
                        var circuitBreakerLogger = loggerFactory.CreateLogger<CircuitBreakerPolicy>();
                        registry.Add(circuitBreaker.PolicyName, circuitBreaker.CircuitBreakerPolicyHandler(circuitBreakerLogger));
                        break;
                    case nameof(AvailablePolicies.TimeoutPolicy):
                        var timeout = policy as TimeoutPolicy;
                        registry.Add(timeout.PolicyName, timeout.TimeoutPolicyHandler());
                        break;
                    case nameof(AvailablePolicies.InMemoryCachePolicy):
                        var inMemoryCache = policy as InMemoryCachePolicy;
                        registry.Add(inMemoryCache.PolicyName, inMemoryCache.InMemoryCachePolicyHandler(memoryCache));
                        break;
                    default:
                        throw new Exception($"Policy {policy.PolicyName} is not currently supported.");
                }
            }

            return registry;
        }
    }
}
