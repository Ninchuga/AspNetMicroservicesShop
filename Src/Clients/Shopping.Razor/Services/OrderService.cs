using Microsoft.Extensions.Logging;
using Shopping.Razor.Extensions;
using Shopping.Razor.Models;
using Shopping.Razor.Responses.Order;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.Razor.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderService> _logger;

        public OrderService(HttpClient httpClient, ILogger<OrderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PlaceOrderResponse> PlaceOrder(BasketCheckout basketCheckout)
        {
            _logger.LogInformation("Calling Order service to place an order.");

            var requestContent = new StringContent(JsonSerializer.Serialize(basketCheckout), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("api/PlaceOrder", requestContent); // gateway api uri

            return new PlaceOrderResponse
            {
                StatusCode = response.StatusCode,
                Success = response.IsSuccessStatusCode,
                ErrorMessage = response.ReasonPhrase
            };
        }

        public async Task<UserOrdersResponse> GetOrdersFor(Guid userId)
        {
            _logger.LogDebug("Getting orders for the user with id: {UserId}", userId);

            var responseMessage = await _httpClient.GetAsync($"api/{userId}"); // call from api gateway
            if (responseMessage.IsSuccessStatusCode)
            {
                var orders = await responseMessage.ReadContentAs<List<UserOrder>>();
                return new UserOrdersResponse { StatusCode = responseMessage.StatusCode, Success = true, UserOrder = orders };
            }
            else
            {
                return new UserOrdersResponse { StatusCode = responseMessage.StatusCode, ErrorMessage = responseMessage.ReasonPhrase, Success = false };
            }
        }
    }
}
