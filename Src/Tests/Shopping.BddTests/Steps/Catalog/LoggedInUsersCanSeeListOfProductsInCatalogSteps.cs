using BoDi;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Shopping.BddTests.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Shopping.BddTests.Steps.Catalog
{
    [Binding]
    public class LoggedInUsersCanSeeListOfProductsInCatalogSteps
    {
        private readonly ScenarioContext _scenarioContext; // put an items in this context and extract them in other steps
        private readonly IObjectContainer _objectContainer;

        public LoggedInUsersCanSeeListOfProductsInCatalogSteps(ScenarioContext scenarioContext, IObjectContainer objectContainer)
        {
            _scenarioContext = scenarioContext;
            _objectContainer = objectContainer;
        }

        [Given(@"a user that is not logged in")]
        public void GivenAUserThatIsNotLoggedIn()
        {
            // Nothing to do
        }

        [When(@"tries to get catalog")]
        public async Task WhenTriesToSeeCatalog()
        {
            var catalogApiHttpClient = _objectContainer.Resolve<HttpClient>("catalogApiHttpClient");

            var response = await catalogApiHttpClient.GetAsync("api/v1/Catalog");
            
            _scenarioContext.Add("CatalogUnauthorizedResponse", response);
            
        }

        [Then(@"unathorized response is returned")]
        public void ThenUnathorizedResponseIsReturned()
        {
            var response = _scenarioContext.Get<HttpResponseMessage>("CatalogUnauthorizedResponse");

            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Given(@"a logged in user")]
        public async Task GivenALoggedInUser()
        {
            var configuration = _objectContainer.Resolve<IConfiguration>("configuration");
            var identityProviderHttpClient = _objectContainer.Resolve<HttpClient>("identityProviderHttpClient");
            var discoveryDocumentResponse = await identityProviderHttpClient.GetDiscoveryDocumentAsync(
                new DiscoveryDocumentRequest
                {
                    Address = configuration["IdentityProviderUrl"],
                    Policy = { ValidateIssuerName = false }
                });
                
            var tokenResponse = await identityProviderHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                ClientId = "shoppingm2m",
                ClientSecret = "m2msecret",
                Scope = "catalogapi.fullaccess" // require multiple scopes
            });
            _scenarioContext.Add("IdentityTokenResponse", tokenResponse);
        }

        [When(@"tries to get products from catalog")]
        public async Task WhenUserTriesToGetProductsFromCatalog()
        {
            var tokenResponse = _scenarioContext.Get<TokenResponse>("IdentityTokenResponse");
            var catalogApiHttpClient = _objectContainer.Resolve<HttpClient>("catalogApiHttpClient");
            catalogApiHttpClient.DefaultRequestHeaders.Clear();
            catalogApiHttpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            catalogApiHttpClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await catalogApiHttpClient.GetAsync("api/v1/Catalog");

            var responseCatalog = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
            _scenarioContext.Add("CatalogProducts", responseCatalog);
        }

        [Then(@"catalog with products is returned")]
        public void ThenCatalogWithProductsIsReturned()
        {
            var catalogProducts = _scenarioContext.Get<IEnumerable<Product>>("CatalogProducts");
            catalogProducts.Should().NotBeEmpty();
        }
    }
}
