using Grpc.Health.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.WebStatus.Grpc.Services
{
    public class DiscountDbServiceStatus
    {
        private readonly Health.HealthClient _healthClient;
        private readonly ILogger<DiscountDbServiceStatus> _logger;

        public DiscountDbServiceStatus(Health.HealthClient healthClient, ILogger<DiscountDbServiceStatus> logger)
        {
            _healthClient = healthClient;
            _logger = logger;
        }

        public async Task<HealthCheckResponse> CheckStatus()
        {
            var request = new HealthCheckRequest { Service = "DiscountDb" };
            var response = new HealthCheckResponse { Status = HealthCheckResponse.Types.ServingStatus.NotServing };

            HttpClient.DefaultProxy = new WebProxy(); // fix for grpc HTTP/2 exception

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
