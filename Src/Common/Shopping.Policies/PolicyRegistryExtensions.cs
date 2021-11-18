using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Shopping.Policies.Models;
using System;

namespace Shopping.Policies
{
    public static class PolicyRegistryExtensions
    {
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


        /// <summary>
        /// Register all available policies on application startup
        /// </summary>
        /// <param name="registry">Registry for policies</param>
        /// <param name="serviceCollection">Service collection for resolving dependencies</param>
        /// <param name="policies">Policies to add to registry</param>
        /// <returns>Registry with added policies</returns>
        public static IPolicyRegistry<string> RegisterPolicies(this IPolicyRegistry<string> registry, IServiceCollection services, params AvailablePolicies[] policies)
        {
            var serviceProvider = services.BuildServiceProvider();
            var policyHolder = serviceProvider.GetRequiredService<IPolicyHolder>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(PolicyRegistryExtensions));
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

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
                        registry.Add(AvailablePolicies.TimeoutPolicy.ToString(), policyHolder.TimeoutPolicy(secondsToWaitForResponse: 10));
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
