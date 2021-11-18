using Polly;
using System.Net.Http;

namespace Shopping.Policies.Models
{
    public class TimeoutPolicy : IAmPolicy
    {
        public string PolicyName { get; }
        public int SecondsToWaitForResponse { get; }

        public TimeoutPolicy(int secondsToWaitForResponse)
        {
            PolicyName = AvailablePolicies.TimeoutPolicy.ToString();
            SecondsToWaitForResponse = secondsToWaitForResponse;
        }

        public IAsyncPolicy<HttpResponseMessage> TimeoutPolicyHandler()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(SecondsToWaitForResponse);
        }
    }
}
