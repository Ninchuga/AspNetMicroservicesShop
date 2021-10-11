using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.Common.Correlations
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string correlationId = string.Empty;

            if(_httpContextAccessor.HttpContext.Items.ContainsKey(CorrelationIdHeader))
            {
                correlationId = _httpContextAccessor.HttpContext.Items[CorrelationIdHeader].ToString();
                request.Headers.Add(CorrelationIdHeader, correlationId);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (!_httpContextAccessor.HttpContext.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                _httpContextAccessor.HttpContext.Response.Headers.Add(CorrelationIdHeader, correlationId);
            }

            return response;
        }
    }
}
