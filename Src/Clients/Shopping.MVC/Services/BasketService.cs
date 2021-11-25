using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models;
using Shopping.MVC.Responses.Basket;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
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

        // In here we are executing distributed transaction using two phase commit protocol
        // where Client will call Basket api which will commit some data and send other data as message to service bus and  bus to Order api
        // Order api will create an order and make a second commit
        // We could also create an additional layer and use Orchestrator/Saga pattern.
        // Checkout Orchestrator will be called from the client instead of calling Basket api directly
        // Orchestrator will call Basket api, if it's successfull call Order Api with phase commit as a single transaction and rollback if there is a failure
        public async Task<BasketCheckoutResponse> Checkout(BasketCheckout basketCheckout)
        {
            _logger.LogInformation("Checking out basket {@BasketCheckout}", basketCheckout);

            var requestContent = new StringContent(JsonSerializer.Serialize(basketCheckout), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Checkout", requestContent); // gateway api uri

            return new BasketCheckoutResponse { Success = response.IsSuccessStatusCode, ErrorMessage = response.ReasonPhrase };
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
                var basket = await responseMessage.ReadContentAs<BasketWithItems>();
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

            var basketItem = new BasketItem
            {
                ProductName = catalogItem.Name,
                ProductId = catalogItem.Id,
                Quantity = catalogItem.Quantity,
                Color = "Red",
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
                var basket = await responseMessage.ReadContentAs<BasketWithItems>();
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
