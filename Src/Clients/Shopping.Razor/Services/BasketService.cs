using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.Razor.Extensions;
using Shopping.Razor.Models;
using Shopping.Razor.Responses.Basket;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.Razor.Services
{
    public class BasketService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BasketService> _logger;

        public BasketService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<BasketService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<BasketResponse> GetBasketFor(Guid userId)
        {
            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());

            //await GetAccessToken();

            _logger.LogDebug("Getting basket for the user id: {UserId}", userId);

            var responseMessage = await _httpClient.GetAsync($"api/{userId}"); // call from api gateway
            if (responseMessage.IsSuccessStatusCode)
            {
                var basket = await responseMessage.ReadContentAs<ShoppingBasket>();
                return new BasketResponse { Success = true, StatusCode = responseMessage.StatusCode, BasketWithItems = basket };
            }
            else
            {
                return new BasketResponse 
                {
                    Success = false,
                    ErrorMessage = responseMessage.ReasonPhrase,
                    StatusCode = responseMessage.StatusCode
                };
            }
        }

        public async Task AddItemToBasket(CatalogItem catalogItem)
        {
            // @ preffix in Serilog is used for complex object
            _logger.LogDebug("Adding catalog item to basket {@CatalogItem}", catalogItem);

            var basketItem = new ShoppingBasketItem
            {
                ProductName = catalogItem.Name,
                ProductId = catalogItem.Id,
                Quantity = catalogItem.Quantity,
                Price = catalogItem.Price * catalogItem.Quantity
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(basketItem), Encoding.UTF8, "application/json");

            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());
            var response = await _httpClient.PostAsync("api/AddBasketItem", requestContent); // gateway api uri
            response.EnsureSuccessStatusCode();
        }

        public async Task<BasketResponse> DeleteBasketItem(string itemId)
        {
            _logger.LogDebug("Deleting basket item with id: {ItemId}", itemId);

            var responseMessage = await _httpClient.DeleteAsync($"api/DeleteBasketItem/{itemId}"); // gateway api uri
            if (responseMessage.IsSuccessStatusCode)
            {
                var basket = await responseMessage.ReadContentAs<ShoppingBasket>();
                return new BasketResponse { Success = true, StatusCode = responseMessage.StatusCode, BasketWithItems = basket };
            }
            else
            {
                return new BasketResponse
                {
                    Success = false,
                    ErrorMessage = responseMessage.ReasonPhrase,
                    StatusCode = responseMessage.StatusCode
                };
            }
        }

        public async Task<DeleteBasketResponse> DeleteBasket(Guid userId)
        {
            _logger.LogDebug("Deleting basket for the user id: {UserId}", userId);

            var response = await _httpClient.DeleteAsync($"api/Delete/{userId}"); // gateway api uri
            return new DeleteBasketResponse 
            { 
                StatusCode = response.StatusCode, 
                ErrorMessage = response.ReasonPhrase, 
                Success = response.IsSuccessStatusCode 
            };
        }

        private async Task<string> GetAccessToken()
        {
            var token = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            return token;

            // Used before API gateway was implemented
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
            //    ClientId = "shopping_web_client",
            //    ClientSecret = "authorizationInteractiveSecret",
            //    Scope = "basketapi.read basketapi.write"
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
