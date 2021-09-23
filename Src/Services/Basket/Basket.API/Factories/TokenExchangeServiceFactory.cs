using Basket.API.Constants;
using Basket.API.Services.Tokens;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;

namespace Basket.API.Factories
{
    public class TokenExchangeServiceFactory : ITokenExchangeServiceFactory
    {
        private readonly Dictionary<string, ITokenExchangeService> _tokenServices;

        public TokenExchangeServiceFactory(IHttpClientFactory _httpClientFactory, IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, IClientAccessTokenCache clientAccessTokenCache)
        {
            var httpClient = _httpClientFactory.CreateClient();
            _tokenServices = new Dictionary<string, ITokenExchangeService>
            {
                { DownstreamServices.DiscountApi, new DiscountTokenService(httpClient, configuration, httpContextAccessor, clientAccessTokenCache) },
                { DownstreamServices.OrderApi, new OrderTokenService(httpClient, configuration, httpContextAccessor, clientAccessTokenCache) }
            };
        }


        // TODO: Since the implementation is identical for both services there is no need for factory
        // we can just pass client token and scopes as parameters to ITokenExchangeService implementation
        public ITokenExchangeService GetTokenExchangeServiceInstance(string serviceName) =>
            _tokenServices.ContainsKey(serviceName)
                    ? _tokenServices[serviceName]
                    : new NullTokenExchangeService();
    }
}
