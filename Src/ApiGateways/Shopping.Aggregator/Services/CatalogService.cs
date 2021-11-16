using Polly;
using Polly.Registry;
using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Models;
using Shopping.Policies;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _cachePolicy;

        public CatalogService(HttpClient httpClient, IPolicyRegistry<string> policyRegistry)
        {
            _httpClient = httpClient;
            _cachePolicy = policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(AvailablePolicies.InMemoryCachePolicy.ToString());
        }

        public async Task<IReadOnlyCollection<CatalogModel>> GetCatalog()
        {
            var response = await ExecuteWithCachePolicy(cacheKey: "Catalog", (context) => _httpClient.GetAsync("/api/v1/Catalog"));
            return response.IsSuccessStatusCode
                ? await response.ReadContentAs<IReadOnlyCollection<CatalogModel>>()
                : new List<CatalogModel>();
        }

        public async Task<CatalogModel> GetCatalogProductBy(string productId)
        {
            var response = await ExecuteWithCachePolicy(cacheKey: $"CatalogItemId-{productId}", (context) => _httpClient.GetAsync($"/api/v1/Catalog/{productId}"));
            return response.IsSuccessStatusCode
                ? await response.ReadContentAs<CatalogModel>()
                : null;
        }

        public async Task<IReadOnlyCollection<CatalogModel>> GetCatalogProductsByCategory(string category)
        {
            var response = await ExecuteWithCachePolicy(cacheKey: $"CatalogItemsByCategory-{category}", 
                (context) => _httpClient.GetAsync($"/api/v1/Catalog/GetProductByCategory/{category}"));

            return response.IsSuccessStatusCode
                ? await response.ReadContentAs<IReadOnlyCollection<CatalogModel>>()
                : new List<CatalogModel>();
        }

        private async Task<HttpResponseMessage> ExecuteWithCachePolicy(string cacheKey, Func<Context, Task<HttpResponseMessage>> func)
        {
            Context policyExecutionContext = new Context(cacheKey);
            var response = await _cachePolicy.ExecuteAsync(func, policyExecutionContext);

            return response;
        }
    }
}
