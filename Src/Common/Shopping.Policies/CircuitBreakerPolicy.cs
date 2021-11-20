using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace Shopping.Policies
{
    public class CircuitBreakerPolicy : IAmPolicy
    {
        public string PolicyName { get; }
        public int AllowedNumberOfAttemptsBeforeBreaking { get; }
        public TimeSpan DurationOfBreak { get; }

        public CircuitBreakerPolicy(int allowedNumberOfAttemptsBeforeBreaking, TimeSpan durationOfBreak)
        {
            PolicyName = AvailablePolicies.CircuitBreakerPolicy.ToString();
            AllowedNumberOfAttemptsBeforeBreaking = allowedNumberOfAttemptsBeforeBreaking;
            DurationOfBreak = durationOfBreak;
        }

        public IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicyHandler(ILogger<CircuitBreakerPolicy> logger)
        {
            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    AllowedNumberOfAttemptsBeforeBreaking,
                    DurationOfBreak,
                    onBreak: (responseMessage, timespan) =>
                    {
                        // circuit break/open can't process requests
                        logger.LogWarning($"There is an error in a downstream service. Circuit is now open for {DurationOfBreak} seconds and it is not allowing calls.");
                    },
                    onReset: () =>
                    {
                        // circuit reset...it gets called only if the request was successfull after the break. 
                        // If not circuit gets opened after a first failed attempt and it waits for time of break duration
                        logger.LogWarning("Trying to recover from a circuit break. Circuit is now closed and can process requests!");
                    }
                );
        }
    }
}
