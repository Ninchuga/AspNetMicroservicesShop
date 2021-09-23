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
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;

        public BasketService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BasketModel> GetBasket(Guid userId)
        {
            var responseMessage = await _httpClient.GetAsync($"/api/v1/Basket/{userId}");
            return await responseMessage.ReadContentAs<BasketModel>();
        }
    }
}
