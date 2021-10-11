using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.Common.Logging
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                _logger.LogHttpResponse(response);

                return response;
            }
            catch(HttpRequestException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
            {
                var hostWithPort = request.RequestUri.IsDefaultPort
                    ? request.RequestUri.DnsSafeHost
                    : $"{request.RequestUri.DnsSafeHost}:{request.RequestUri.Port}";

                _logger.LogCritical(ex, "Unable to connect to {Host}. Please check the configuration to ensure correct URL has been configured.", hostWithPort);
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway)
            {
                RequestMessage = request
            };
        }
    }
}
