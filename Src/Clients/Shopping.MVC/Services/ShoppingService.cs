using Microsoft.Extensions.Logging;
using Shopping.MVC.Extensions;
using Shopping.MVC.Models.Aggregator;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
{
    public class ShoppingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShoppingService> _logger;

        public ShoppingService(HttpClient httpClient, ILogger<ShoppingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserShoppingDetails> GetUserShoppingDetails(Guid userId)
        {
            _logger.LogDebug("Getting shopping details for the user with id: {UserId}", userId);

            var responseMessage = await _httpClient.GetAsync($"api/v1/Shopping/GetUserShoppingDetails/{userId}"); // calling aggregator

            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.ReadContentAs<UserShoppingDetails>();
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
