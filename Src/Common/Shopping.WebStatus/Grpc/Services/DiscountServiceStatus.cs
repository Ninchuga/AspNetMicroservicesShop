using Grpc.Health.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Shopping.WebStatus.Grpc.Services
{
    public class DiscountServiceStatus
    {
        private readonly Health.HealthClient _healthClient;
        private readonly ILogger<DiscountServiceStatus> _logger;

        public DiscountServiceStatus(Health.HealthClient healthClient, ILogger<DiscountServiceStatus> logger)
        {
            _healthClient = healthClient;
            _logger = logger;
        }

        public async Task<HealthCheckResponse> CheckStatus()
        {
            var request = new HealthCheckRequest { Service = "Discount" };
            var response = new HealthCheckResponse { Status = HealthCheckResponse.Types.ServingStatus.NotServing };

            // in case that gRpc service is not yet up and running
            // or Service name doesn't exist, CheckAsync() will throw exception
            try
            {
                var health = await _healthClient.CheckAsync(request);

                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return response;
        }
    }
}
