using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.HealthChecks
{
    public class DownstreamApiHealthCheck : IHealthCheck
    {
        private readonly Uri _apiEndpoint;
        private readonly HttpClient _httpClient;

        public DownstreamApiHealthCheck(Uri apiEndpoint)
        {
            _apiEndpoint = apiEndpoint;
            _httpClient = new HttpClient();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(_apiEndpoint.AbsoluteUri);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy();
            }

            return new HealthCheckResult(context.Registration.FailureStatus);
        }
    }
}
