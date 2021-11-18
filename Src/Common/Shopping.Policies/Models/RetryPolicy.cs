using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace Shopping.Policies.Models
{
    public class RetryPolicy : IAmPolicy
    {
        public string PolicyName { get; }
        public int RetryCount { get; }

        public RetryPolicy(int retryCount)
        {
            PolicyName = AvailablePolicies.RetryPolicy.ToString();
            RetryCount = retryCount;
        }

        public IAsyncPolicy<HttpResponseMessage> RetryPolicyHandler(ILogger<RetryPolicy> logger)
        {
            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                //HttpPolicyExtensions
                //.HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                     retryCount: RetryCount,
                     sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                     onRetry: (exception, retryCount, context) =>
                     {
                         logger.LogWarning($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception.Exception?.Message}.");
                     }
                 );
        }

        public Func<int, TimeSpan> SleepDuration()
        {
            return retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        }

    }
}
