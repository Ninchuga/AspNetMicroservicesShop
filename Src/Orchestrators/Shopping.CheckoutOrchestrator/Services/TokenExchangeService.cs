using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Services
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

        public async Task<string> ExchangeAccessToken(string tokenExchangeCacheKey, string serviceScopes, string subjectAccessToken)
        {
            // GetAsync() will only return access token if it's not expired
            var item = await _clientAccessTokenCache.GetAsync(tokenExchangeCacheKey);
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
                { "subject_token", subjectAccessToken }, // subject_token is an access token passed from publisher of the event (order, payment, delivery apis)
                { "scope", $"openid profile {serviceScopes}" }
            };

            var tokenResponse = await _httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange", // token exchange grant type
                Parameters = new Parameters(customParams),
                ClientId = "ordersagaorchestratortokenexchangeclient",
                ClientSecret = "ordersagaorchestratortokenexchangeclientsecret",
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
