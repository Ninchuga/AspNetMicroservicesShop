using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using System;

namespace Shopping.Policies
{
    public static class PolicyRegistryExtensions
    {
        /// <summary>
        /// Register all available policies on application startup
        /// </summary>
        /// <param name="registry">Registry for policies</param>
        /// <param name="serviceCollection">Service collection for resolving dependencies</param>
        /// <param name="policies">Policies to add to registry</param>
        /// <returns>Registry with added policies</returns>
        public static IPolicyRegistry<string> RegisterPolicies(this IPolicyRegistry<string> registry, IServiceCollection serviceCollection, params AvailablePolicies[] policies)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var policyHolder = serviceProvider.GetRequiredService<IPolicyHolder>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            var logger = loggerFactory.CreateLogger(nameof(PolicyRegistryExtensions));

            foreach (var policy in policies)
            {
                switch (policy)
                {
                    case AvailablePolicies.FallbackPolicy:
                        registry.Add(AvailablePolicies.FallbackPolicy.ToString(), policyHolder.FallbackPolicy());
                        break;
                    case AvailablePolicies.RetryPolicy:
                        registry.Add(AvailablePolicies.RetryPolicy.ToString(), policyHolder.RetryPolicy(retryCount: 3));
                        break;
                    case AvailablePolicies.CircuitBreakerPolicy:
                        registry.Add(AvailablePolicies.CircuitBreakerPolicy.ToString(), 
                            policyHolder.CircuitBreakerPolicy(allowedNumberOfAttemptsBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(10)));
                        break;
                    case AvailablePolicies.TimeoutPolicy:
                        registry.Add(AvailablePolicies.TimeoutPolicy.ToString(), policyHolder.TimeoutPolicy(secondsToWaitForResponse: 15));
                        break;
                    case AvailablePolicies.InMemoryCachePolicy:
                        registry.Add(AvailablePolicies.InMemoryCachePolicy.ToString(), policyHolder.InMemoryCachePolicy(memoryCache, ttl: TimeSpan.FromMinutes(5)));
                        break;
                    default:
                        logger.LogWarning("Policy {PolicyName} is not supported in this method.", nameof(policy));
                        break;
                }
            }

            return registry;
        }
    }
}
