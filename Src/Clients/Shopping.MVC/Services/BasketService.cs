using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models;
using Shopping.MVC.Responses;
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

        public BasketService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BasketCheckoutResponse> Checkout(BasketCheckout basketCheckout)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(basketCheckout), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Checkout", requestContent); // gateway api uri

            return new BasketCheckoutResponse { Success = response.IsSuccessStatusCode, ErrorMessage = response.ReasonPhrase };
        }

        public async Task<BasketWithItems> GetBasketFor(Guid userId)
        {
            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());

            var responseMessage = await _httpClient.GetAsync($"api/{userId}"); // call from api gateway

            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.ReadContentAs<BasketWithItems>();
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return null;
            }

            throw new Exception("Unhadled exception occurred while retreiving catalog");
        }

        public async Task AddItemToBasket(CatalogItem catalogItem)
        {
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

        public async Task<BasketWithItems> DeleteBasketItem(string itemId)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/DeleteBasketItem/{itemId}"); // gateway api uri

            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.ReadContentAs<BasketWithItems>();
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return null;
            }

            throw new Exception("Unhadled exception occurred while retreiving catalog");
        }

        public async Task DeleteBasket(Guid userId)
        {
            var response = await _httpClient.DeleteAsync($"api/Delete/{userId}"); // gateway api uri
            response.EnsureSuccessStatusCode();
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
