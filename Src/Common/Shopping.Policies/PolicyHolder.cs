using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Shopping.Policies
{
    public class PolicyHolder : IPolicyHolder
    {
        private readonly ILogger<PolicyHolder> _logger;
        private readonly int _cachedResult = 0;

        public PolicyHolder(ILogger<PolicyHolder> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> FallbackPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                });
        }

        public IAsyncPolicy<HttpResponseMessage> RetryPolicy(int retryCount)
        {
            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                     retryCount: retryCount,
                     sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                     onRetry: (exception, retryCount, context) =>
                     {
                         _logger.LogWarning($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                     }
                 );
        }

        public IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy(int allowedNumberOfAttemptsBeforeBreaking, TimeSpan durationOfBreak)
        {
            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    allowedNumberOfAttemptsBeforeBreaking,
                    durationOfBreak,
                    onBreak: (responseMessage, timespan) =>
                    {
                        // circuit break
                    },
                    onReset: () =>
                    {
                        // circuit reset
                    }
                );
        }

        public IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(int secondsToWaitForResponse)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(secondsToWaitForResponse);
        }

        public IPolicyWrap<HttpResponseMessage> FallbackRetryCircuitAndTimeoutWrap()
        {
            return Policy.WrapAsync(FallbackPolicy(), RetryPolicy(3), CircuitBreakerPolicy(3, TimeSpan.FromSeconds(10)), TimeoutPolicy(1));
        }

        public IAsyncPolicy<HttpResponseMessage> InMemoryCachePolicy(IMemoryCache memoryCache, TimeSpan ttl)
        {
            Polly.Caching.Memory.MemoryCacheProvider memoryCacheProvider = new Polly.Caching.Memory.MemoryCacheProvider(memoryCache);

            return Policy.CacheAsync<HttpResponseMessage>(memoryCacheProvider, ttl);
        }
    }
}
