using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OcelotApiGateway.DelegatingHandlers
{
    public class TokenExchangeDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly ILogger<TokenExchangeDelegatingHandler> _logger;
        //private const string BasketApiTokenExchangeCacheKey = "gatewayandaggregatortodownstreamtokenexchangeclient_basketapi";

        public TokenExchangeDelegatingHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, 
            IClientAccessTokenCache clientAccessTokenCache, ILogger<TokenExchangeDelegatingHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _clientAccessTokenCache = clientAccessTokenCache;
            _logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // extract the current token
            string incomingToken = request.Headers.Authorization.Parameter;

            var (tokenExchangeCacheKey, downstreamServiceScopes) = ResolveDownstreamServicesTokenExchangeCacheKeyAndScopes(request.RequestUri.AbsoluteUri, _configuration);

            var accessToken = await GetAccessToken(incomingToken, tokenExchangeCacheKey, downstreamServiceScopes);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // exchange it
            //var newToken = await ExchangeToken(incomingToken);

            // replace incoming bearer token with our new one
            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

            return await base.SendAsync(request, cancellationToken);
        }

        // Return non expired access token from the cache
        // If it doesn't exist call IndetityService for another one
        private async Task<string> GetAccessToken(string incomingToken, string tokenExchangeCacheKey, string downstreamServiceScopes)
        {
            _logger.LogInformation("Getting access token...");

            // GetAsync() will only return access token if it's not expired
            var item = await _clientAccessTokenCache.GetAsync(tokenExchangeCacheKey); // prepend audience name of the downstream service to the ClientId
            if (item != null)
            {
                return item.AccessToken;
            }

            _logger.LogInformation("Exchanging token...");

            var (accessToken, expiresIn) = await ExchangeToken(incomingToken, downstreamServiceScopes);

            _logger.LogInformation("Token exchanged successfully.");

            await _clientAccessTokenCache.SetAsync(tokenExchangeCacheKey, accessToken, expiresIn);

            return accessToken;
        }

        private async Task<(string, int)> ExchangeToken(string incomingToken, string downstreamServiceScopes)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // use discovery document from IdentityModel package to access token endpoint from IdentityService
            var discoveryDocumentResponse = await httpClient.GetDiscoveryDocumentAsync(_configuration["IdentityProviderSettings:IdentityServiceUrl"]);
            if (discoveryDocumentResponse.IsError)
            {
                _logger.LogError("Discovery document response has error.");
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "subject_token", incomingToken }, // subject_token is an access token passed from the Client App (MVC)
                { "scope", downstreamServiceScopes }
            };

            _logger.LogInformation("Requesting access token from {TokenEndpoint}", discoveryDocumentResponse.TokenEndpoint);

            var tokenResponse = await httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange", // token exchange grant type
                Parameters = new Parameters(customParams),
                ClientId = "gatewayandaggregatortodownstreamtokenexchangeclient",
                ClientSecret = "379a2304-28d6-486e-bec4-862f4bb0bf88",
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError("Requested token failed to be retreived.");
                throw new Exception(tokenResponse.Error);
            }

            return (tokenResponse.AccessToken, tokenResponse.ExpiresIn);
        }

        private (string, string) ResolveDownstreamServicesTokenExchangeCacheKeyAndScopes(string requestUri, IConfiguration configuration)
        {
            if (requestUri.Contains("Catalog", StringComparison.OrdinalIgnoreCase))
                return (configuration["DownstreamServicesTokenExhangeCacheKeys:CatalogApi"], configuration["DownstreamServicesScopes:CatalogApi"]);
            if(requestUri.Contains("Basket", StringComparison.OrdinalIgnoreCase))
                return (configuration["DownstreamServicesTokenExhangeCacheKeys:BasketApi"], configuration["DownstreamServicesScopes:BasketApi"]);
            if (requestUri.Contains("Order", StringComparison.OrdinalIgnoreCase))
                return (configuration["DownstreamServicesTokenExhangeCacheKeys:OrderApi"], configuration["DownstreamServicesScopes:OrderApi"]);

            return (string.Empty, string.Empty);
        }
    }
}
