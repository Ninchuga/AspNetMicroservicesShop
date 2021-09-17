using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;

        public CatalogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<CatalogModel>> GetCatalog()
        {
            var responseMessage = await _httpClient.GetAsync("/api/v1/Catalog");
            return await responseMessage.ReadContentAs<List<CatalogModel>>();
        }

        public async Task<CatalogModel> GetCatalog(string id)
        {
            var responseMessage = await _httpClient.GetAsync($"/api/v1/Catalog/{id}");
            return await responseMessage.ReadContentAs<CatalogModel>();
        }

        public async Task<IEnumerable<CatalogModel>> GetCatalogByCategory(string category)
        {
            var responseMessage = await _httpClient.GetAsync($"/api/v1/Catalog/GetProductByCategory/{category}");
            return await responseMessage.ReadContentAs<List<CatalogModel>>();
        }
    }
}
