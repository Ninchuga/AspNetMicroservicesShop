using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shopping.WebStatus.Grpc.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.WebStatus.Grpc.HealthChecks
{
    public class DiscountServiceHealthCheck : IHealthCheck
    {
        private readonly DiscountServiceStatus _discountStatusService;

        public DiscountServiceHealthCheck(DiscountServiceStatus discountStatusService)
        {
            _discountStatusService = discountStatusService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var health = await _discountStatusService.CheckStatus();

            return health.Status switch
            {
                HealthCheckResponse.Types.ServingStatus.ServiceUnknown or HealthCheckResponse.Types.ServingStatus.Unknown => HealthCheckResult.Unhealthy(),
                HealthCheckResponse.Types.ServingStatus.Serving => HealthCheckResult.Healthy(),
                HealthCheckResponse.Types.ServingStatus.NotServing => HealthCheckResult.Degraded(),
                _ => HealthCheckResult.Unhealthy(),
            };
        }
    }
}
