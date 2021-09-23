﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<CatalogItem>> GetCatalog()
        {
            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());
            var responseMessage = await _httpClient.GetAsync("api"); // route is already configured from HttpClient middleware

            if(responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.ReadContentAs<List<CatalogItem>>();
            }
            else if(responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return null;
            }

            throw new Exception("Unhadled exception occurred while retreiving catalog");
        }

        public async Task<CatalogItem> GetCatalogItemBy(string itemId)
        {
            // we don't need this when we are using AddUserAccessTokenHandler() for refresh token flow
            // this handler do all the work for us
            //_httpClient.SetBearerToken(await GetAccessToken());
            //await GetAccessToken();
            var responseMessage = await _httpClient.GetAsync($"api/{itemId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.ReadContentAs<CatalogItem>();
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return null;
            }

            throw new Exception("Unhadled exception occurred while retreiving catalog");
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
