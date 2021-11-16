using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Contracts;
using Shopping.Aggregator.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ShoppingController : ControllerBase
    {
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public ShoppingController(IBasketService basketService, IOrderService orderService)
        {
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [Authorize]
        [HttpGet]
        [Route("[action]/{userId}")]
        [ProducesResponseType(typeof(ShoppingModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingModel>> GetUserShoppingDetails(Guid userId)
        {
            // get basket with userId
            // iterate basket items and consume products with basket item productId member
            // map product related members into basketitem dto with extended columns
            // consume ordering microservices in order to retrieve order list
            // return root ShoppngModel dto class which including all responses

            var basket = await _basketService.GetBasket(userId);
            var orders = await _orderService.GetOrdersBy(userId);

            var shoppingModel = new ShoppingModel
            {
                BasketWithProducts = basket,
                Orders = orders
            };

            return Ok(shoppingModel);
        }
    }
}
