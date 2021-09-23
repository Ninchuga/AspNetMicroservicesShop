using Shopping.MVC.Extensions;
using Shopping.MVC.Models.Aggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shopping.MVC.Services
{
    public class ShoppingService
    {
        private readonly HttpClient _httpClient;

        public ShoppingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserShoppingDetails> GetUserShoppingDetails(Guid userId)
        {
            var responseMessage = await _httpClient.GetAsync($"api/v1/Shopping/GetUserShoppingDetails/{userId}"); // call from api gateway

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
