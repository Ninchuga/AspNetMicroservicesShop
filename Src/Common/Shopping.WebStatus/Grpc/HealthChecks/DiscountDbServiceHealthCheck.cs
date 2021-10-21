using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shopping.WebStatus.Grpc.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.WebStatus.Grpc.HealthChecks
{
    public class DiscountDbServiceHealthCheck : IHealthCheck
    {
        private readonly DiscountDbServiceStatus _discountDbService;

        public DiscountDbServiceHealthCheck(DiscountDbServiceStatus discountDbService)
        {
            _discountDbService = discountDbService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var health = await _discountDbService.CheckStatus();

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
