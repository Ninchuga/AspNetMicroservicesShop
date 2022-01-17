using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Payment.API.Services
{
    public class TokenExchangeService : ITokenExchangeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;

        public TokenExchangeService(HttpClient httpClient, IConfiguration configuration, IClientAccessTokenCache clientAccessTokenCache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _clientAccessTokenCache = clientAccessTokenCache;
        }

        public async Task<string> ExchangeAccessToken(string tokenExchangeCacheKey, string downstreamServiceScopes, string subjectAccessToken)
        {
            // GetAsync() will only return access token if it's not expired
            var item = await _clientAccessTokenCache.GetAsync(tokenExchangeCacheKey); // prepend audience name of the downstream service to the ClientId
            if (item != null)
            {
                return item.AccessToken;
            }

            var discoveryDocumentResponse = await _httpClient.GetDiscoveryDocumentAsync(_configuration["IdentityProviderSettings:IdentityServiceUrl"]);
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "subject_token", subjectAccessToken }, // subject_token is an access token passed from publisher of the event (saga orchestrator)
                { "scope", $"openid profile {downstreamServiceScopes}" }
            };

            var tokenResponse = await _httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                Parameters = new Parameters(customParams),
                ClientId = "downstreamservicestokenexchangeclient",
                ClientSecret = "downstreamtokenexchangesecret",
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            await _clientAccessTokenCache.SetAsync(
                tokenExchangeCacheKey,
                tokenResponse.AccessToken,
                tokenResponse.ExpiresIn);

            return tokenResponse.AccessToken;
        }
    }
}
