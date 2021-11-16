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

            UserShoppingDetails userShoppingDetails;

            var responseMessage = await _httpClient.GetAsync($"api/v1/Shopping/GetUserShoppingDetails/{userId}"); // calling aggregator
            if (responseMessage.IsSuccessStatusCode)
            {
                userShoppingDetails = await responseMessage.ReadContentAs<UserShoppingDetails>();
                userShoppingDetails.Success = true;
                userShoppingDetails.StatusCode = responseMessage.StatusCode;
            }
            else
            {
                userShoppingDetails = new UserShoppingDetails
                {
                    StatusCode = responseMessage.StatusCode,
                    Success = false,
                    ErrorMessage = responseMessage.ReasonPhrase
                };
            }

            return userShoppingDetails;
        }
    }
}
