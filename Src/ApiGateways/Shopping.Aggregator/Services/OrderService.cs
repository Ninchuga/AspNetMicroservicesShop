using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<OrderResponseModel>> GetOrdersBy(Guid userId)
        {
            var responseMessage = await _httpClient.GetAsync($"/api/v1/Order/{userId}");
            return responseMessage.IsSuccessStatusCode
                ? await responseMessage.ReadContentAs<IReadOnlyCollection<OrderResponseModel>>()
                : new List<OrderResponseModel>();
        }
    }
}
