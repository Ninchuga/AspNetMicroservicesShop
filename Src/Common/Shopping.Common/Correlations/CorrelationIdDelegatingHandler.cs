using Microsoft.AspNetCore.Http;
using Shopping.Common.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shopping.Common.Correlations
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string correlationId = string.Empty;

            if(_httpContextAccessor.HttpContext.Items.ContainsKey(Headers.CorrelationIdHeader))
            {
                correlationId = _httpContextAccessor.HttpContext.Items[Headers.CorrelationIdHeader].ToString();
                request.Headers.Add(Headers.CorrelationIdHeader, correlationId);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (!_httpContextAccessor.HttpContext.Response.Headers.ContainsKey(Headers.CorrelationIdHeader))
            {
                _httpContextAccessor.HttpContext.Response.Headers.Add(Headers.CorrelationIdHeader, correlationId);
            }

            return response;
        }
    }
}
