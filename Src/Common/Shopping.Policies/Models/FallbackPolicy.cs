using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.Policies.Models
{
    public class FallbackPolicy : IAmPolicy
    {
        public string PolicyName { get; }
        private ILogger<FallbackPolicy> _logger;
        private readonly HttpStatusCode _httpStatusCodeToFallback;

        public FallbackPolicy()
        {
            PolicyName = AvailablePolicies.FallbackPolicy.ToString();
        }

        public FallbackPolicy(HttpStatusCode httpStatusCode)
        {
            PolicyName = AvailablePolicies.FallbackPolicy.ToString();
            _httpStatusCodeToFallback = httpStatusCode;
        }

        public IAsyncPolicy<HttpResponseMessage> FallbackPolicyHandler(ILogger<FallbackPolicy> logger)
        {
            _logger = logger;

            return Policy.HandleResult<HttpResponseMessage>(result => !result.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .FallbackAsync(FallbackAction, OnFallbackAsync);
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

            if(_httpStatusCodeToFallback != 0)
            {
                _logger.LogWarning($"Executing fallback action with response status code {responseToFailedRequest.Result.StatusCode}");
                httpResponseMessage = new HttpResponseMessage(_httpStatusCodeToFallback);
            }
            else if (responseToFailedRequest.Result != null)
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
                    : new StringContent($"The fallback executed with {HttpStatusCode.InternalServerError}")
                };
            }

            return Task.FromResult(httpResponseMessage);
        }
    }
}
