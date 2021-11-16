using Microsoft.Extensions.Logging;
using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Services
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;
        private readonly ICatalogService _catalogService;
        private readonly ILogger<BasketService> _logger;

        public BasketService(HttpClient httpClient, ICatalogService catalogService, ILogger<BasketService> logger)
        {
            _httpClient = httpClient;
            _catalogService = catalogService;
            _logger = logger;
        }

        public async Task<BasketModel> GetBasket(Guid userId)
        {
            BasketModel basket = new BasketModel();

            var responseMessage = await _httpClient.GetAsync($"/api/v1/Basket/{userId}");
            if(responseMessage.IsSuccessStatusCode)
            {
                basket = await responseMessage.ReadContentAs<BasketModel>();
                foreach (var item in basket.Items)
                {
                    var product = await _catalogService.GetCatalogProductBy(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogDebug("Product {ProductId} in the basket for the user {UserId} could not be found.", item.ProductId, userId);
                        continue;
                    }

                    // set additional product fields onto basket item
                    item.ProductName = product.Name;
                    item.Category = product.Category;
                    item.Summary = product.Summary;
                    item.Description = product.Description;
                    item.ImageFile = product.ImageFile;
                }
            }

            return basket;
        }
    }
}
