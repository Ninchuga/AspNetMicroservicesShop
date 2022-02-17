using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Basket.API.Services.Tokens
{
    public class TokenExchangeService : ITokenExchangeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;

        public TokenExchangeService(HttpClient httpClient,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IClientAccessTokenCache clientAccessTokenCache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _clientAccessTokenCache = clientAccessTokenCache;
        }

        public async Task<string> GetAccessTokenForDownstreamService(string downstreamApiAccessTokenCacheKey, string downstreamApiScopes)
        {
            // GetAsync() will only return access token if it's not expired
            var item = await _clientAccessTokenCache.GetAsync(downstreamApiAccessTokenCacheKey); // prepend audience name of the downstream service to the ClientId
            if (item != null)
            {
                return item.AccessToken;
            }

            var discoveryDocumentResponse = await _httpClient.GetDiscoveryDocumentAsync(_configuration["IdentityProviderSettings:IdentityServiceUrl"]);

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }
            var subjectToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "subject_token", subjectToken }, // subject_token is an access token passed to Basket API from the Client App (MVC)
                { "scope", $"openid profile {downstreamApiScopes}" }
            };

            var tokenResponse = await _httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange", // token exchange grant type
                Parameters = new Parameters(customParams),
                ClientId = "downstreamservicestokenexchangeclient",
                ClientSecret = "downstreamtokenexchangesecret",
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            await _clientAccessTokenCache.SetAsync(
                downstreamApiAccessTokenCacheKey,
                tokenResponse.AccessToken,
                tokenResponse.ExpiresIn);

            return tokenResponse.AccessToken;
        }
    }
}
