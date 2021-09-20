using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
{
    public class BasketService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _accessToken;

        public BasketService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BasketWithItems> GetBasketFor(Guid userId)
        {
            _httpClient.SetBearerToken(await GetAccessToken());

            //var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            //_httpClient.SetBearerToken(accessToken);
            var responseMessage = await _httpClient.GetAsync($"/api/v1/Basket/{userId}");

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
            //var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            // extract User Id from 'sub' (subject) claim
            var userId =
                    Guid.Parse(
                        _httpContextAccessor.HttpContext
                        .User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            var basket = new BasketWithItems
            {
                UserId = userId,
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        ProductName = catalogItem.Name,
                        ProductId = catalogItem.Id,
                        Quantity = catalogItem.Quantity,
                        Color = "Red",
                        Price = catalogItem.Price
                    }
                }
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(basket), Encoding.UTF8, "application/json");

            _httpClient.SetBearerToken(await GetAccessToken());
            var response = await _httpClient.PostAsync("/api/v1/Basket", requestContent);
            response.EnsureSuccessStatusCode();
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
