using FluentAssertions;
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
        private readonly HttpClient _httpClient;
        private readonly ScenarioContext _scenarioContext; // put an items in this context and extract them in other steps

        public LoggedInUsersCanSeeListOfProductsInCatalogSteps(HttpClient httpClient, ScenarioContext scenarioContext)
        {
            _httpClient = httpClient;
            _scenarioContext = scenarioContext;
        }

        [Given(@"a user that is not logged in")]
        public void GivenAUserThatIsNotLoggedIn()
        {
            //ScenarioContext.StepIsPending();
        }

        [When(@"tries to see catalog")]
        public async Task WhenTriesToSeeCatalog()
        {
            //ScenarioContext.StepIsPending();
            var response = await _httpClient.GetAsync("/api/v1/Catalog");
            //var responseCatalog = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
            _scenarioContext.Add("CatalogUnauthorizedResponse", response);
            //_scenarioContext.Add("CatalogProducts", responseCatalog);
        }

        [Then(@"unathorized response is returned")]
        public void ThenUnathorizedResponseIsReturned()
        {
            //ScenarioContext.StepIsPending();
            //var catalog = _scenarioContext.Get<IEnumerable<Product>>("Catalog");
            var response = _scenarioContext.Get<HttpResponseMessage>("CatalogUnauthorizedResponse");
            //var catalogProducts = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.Content.Should().BeNull();

        }

        [Given(@"a logged in user")]
        public void GivenALoggedInUser()
        {
            ScenarioContext.StepIsPending();
        }
        
        [When(@"tries to get catalog")]
        public void WhenUserTriesToGetCatalog()
        {
            ScenarioContext.StepIsPending();
        }
        
        [Then(@"catalog with products is displayed")]
        public void ThenCatalogWithProductsIsDisplayed()
        {
            ScenarioContext.StepIsPending();
        }
    }
}
