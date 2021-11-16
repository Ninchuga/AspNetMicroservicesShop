using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models;
using Shopping.MVC.Responses.Catalog;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CatalogService> _logger;

        public CatalogService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<CatalogService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<GetCatalogResponse> GetCatalog()
        {
            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());

            var responseMessage = await _httpClient.GetAsync("api"); // route is already configured from HttpClient middleware
            if (responseMessage.IsSuccessStatusCode)
            {
                var catalogItems = await responseMessage.ReadContentAs<List<CatalogItem>>();
                return new GetCatalogResponse { Success = true, CatalogItems = catalogItems, StatusCode = responseMessage.StatusCode };
            }
            else
            {
                return new GetCatalogResponse
                {
                    Success = false,
                    CatalogItems = new List<CatalogItem>(),
                    ErrorMessage = responseMessage.ReasonPhrase,
                    StatusCode = responseMessage.StatusCode
                };
            }
        }

        public async Task<GetCatalogItemResponse> GetCatalogItemBy(string itemId)
        {
            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());
            //await GetAccessToken();

            _logger.LogDebug("Calling catalog api to return item with id {ItemId}", itemId);

            var responseMessage = await _httpClient.GetAsync($"api/{itemId}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var item = await responseMessage.ReadContentAs<CatalogItem>();

                _logger.LogDebug("Successfully loaded item {ItemName}", item.Name);

                return new GetCatalogItemResponse { Success = true, StatusCode = responseMessage.StatusCode, CatalogItem = item };
            }
            else
            {
                return new GetCatalogItemResponse
                {
                    Success = false,
                    ErrorMessage = responseMessage.ReasonPhrase,
                    StatusCode = responseMessage.StatusCode
                };
            }
        }

        private async Task<string> GetAccessToken()
        {
            var token = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            return token;

            //if (!string.IsNullOrWhiteSpace(_accessToken))
            //    return _accessToken;

            //var discoveryDocumentResponse = await _httpClient.GetDiscoveryDocumentAsync(_configuration["IdentityProviderSettings:IdentityServiceUrl"]);

            //if (discoveryDocumentResponse.IsError)
            //{
            //    throw new Exception(discoveryDocumentResponse.Error);
            //}

            //var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            //{
            //    Address = discoveryDocumentResponse.TokenEndpoint,
            //    ClientId = "shopping_web_client", // "shoppingm2m",
            //    ClientSecret = "authorizationInteractiveSecret", //"m2msecret",
            //    Scope = "catalogapi.read catalogapi.fullaccess" // require multiple scopes
            //});

            //if (tokenResponse.IsError)
            //{
            //    throw new Exception(tokenResponse.Error);
            //}

            //_accessToken = tokenResponse.AccessToken;
            //return _accessToken;
        }
    }
}
