using Microsoft.Extensions.Logging;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
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

        public async Task<List<UserOrder>> GetOrdersFor(Guid userId)
        {
            _logger.LogDebug("Getting orders for the user with id: {UserId}", userId);

            var responseMessage = await _httpClient.GetAsync($"api/{userId}"); // call from api gateway

            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.ReadContentAs<List<UserOrder>>();
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return null;
            }

            throw new Exception("Unhadled exception occurred while retreiving catalog");
        }
    }
}
