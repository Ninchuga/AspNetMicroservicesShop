using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Shopping.Logging
{
    public static class LoggerExtensions
    {
        public static void LogHttpResponse(this ILogger logger, HttpResponseMessage responseMessage)
        {
            if(responseMessage.IsSuccessStatusCode)
            {
                logger.LogDebug("Received a success response from {Url}", responseMessage.RequestMessage.RequestUri);
            }
            else
            {
                logger.LogWarning("Received a non-success status code {StatusCode} from {Url}",
                    (int)responseMessage.StatusCode, responseMessage.RequestMessage.RequestUri);
            }

        }
    }
}
