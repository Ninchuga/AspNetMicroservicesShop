using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OcelotApiGateway.DelegatingHandlers
{
    public class BasketApiTokenExchangeDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;


        public BasketApiTokenExchangeDelegatingHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // extract the current token
            var incomingToken = request.Headers.Authorization.Parameter;

            // exchange it
            var newToken = await ExchangeToken(incomingToken);

            // replace incoming bearer token with our new one
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> ExchangeToken(string incomingToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // use discovery document from IdentityModel package to access token endpoint from IdentityService
            var discoveryDocumentResponse = await httpClient.GetDiscoveryDocumentAsync(_configuration["IdentityProviderSettings:IdentityServiceUrl"]);

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "subject_token", incomingToken }, // subject_token is an access token passed from the Client App (MVC)
                { "scope", "openid profile basketapi.fullaccess" }
            };

            var tokenResponse = await httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange", // token exchange grant type
                Parameters = new Parameters(customParams),
                ClientId = "gatewaytodownstreamtokenexchangeclient",
                ClientSecret = "379a2304-28d6-486e-bec4-862f4bb0bf88",
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            var accessToken = tokenResponse.AccessToken;
            return accessToken;
        }
    }
}
