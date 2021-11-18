using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

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
                .Or<BrokenCircuitException>()
                .FallbackAsync(FallbackAction, OnFallbackAsync);
        }

        public IAsyncPolicy<HttpResponseMessage> FallbackPolicy<TResponse>(HttpStatusCode statusCode, TResponse response) where TResponse : class
        {
            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .FallbackAsync(new HttpResponseMessage(statusCode)
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
                         _logger.LogWarning($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception.Exception.Message}.");
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
                        // circuit break/open
                        _logger.LogWarning($"There is an error in a downstream service. Circuit is now open for {durationOfBreak} seconds and it is not allowing calls.");
                    },
                    onReset: () =>
                    {
                        // circuit reset
                        _logger.LogWarning("Trying to recover from a circuit break. Circuit is now closed and can process requests!");
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

        public IAsyncPolicy<HttpResponseMessage> WrapPolicies(params IAsyncPolicy<HttpResponseMessage>[] policies)
        {
            return Policy.WrapAsync(policies);
        }

        private Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context)
        {
            if (response.Exception != null)
                _logger.LogWarning($"Fallback action triggered. Exception occurred while executing the request. Exception message: {response.Exception.Message}");

            return Task.CompletedTask;
        }

        private Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest, Context context, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponseMessage;

            if (responseToFailedRequest.Result != null)
            {
                _logger.LogWarning($"Executing fallback action with response status code {responseToFailedRequest.Result.StatusCode} and reason: {responseToFailedRequest.Result.ReasonPhrase}");
                httpResponseMessage = new HttpResponseMessage(responseToFailedRequest.Result.StatusCode)
                {
                    Content = new StringContent($"The fallback executed, the original error was {responseToFailedRequest.Result.ReasonPhrase}")
                };
            }
            else
            {
                _logger.LogWarning($"Executing fallback action with response status code: {HttpStatusCode.InternalServerError} and exception message: {responseToFailedRequest.Exception.Message}");
                httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = responseToFailedRequest.Exception != null
                    ? new StringContent($"The fallback executed, the original error was {responseToFailedRequest.Exception.Message}")
                    : new StringContent($"The fallback executed with https status code {HttpStatusCode.InternalServerError}")
                };
            }

            return Task.FromResult(httpResponseMessage);
        }
    }
}
