using Basket.API.DTOs;
using Basket.API.Entities;
using Basket.API.Services.Basket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly ILogger<BasketController> _logger;
        private readonly IBasketService _basketService;

        public BasketController(ILogger<BasketController> logger, IBasketService basketService)
        {
            _logger = logger;
            _basketService = basketService;
        }

        [HttpGet("{userId}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(Guid userId)
        {
            // UserId passed from the API Gateway through request headers
            // There token was taken from sub claim of the authenticated user
            //Guid usId = Guid.Parse(HttpContext.Request.Headers["CurrentUser"][0]);

            // Now that we are passing scopes from the API Gateway we can extract this info from the Claims object
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid usrId);

            return Ok(await _basketService.GetBasketBy(userId));
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            return Ok(await _basketService.UpdateBasket(basket));
        }

        [HttpDelete()]
        [Route("[action]/{itemId}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> DeleteBasketItem(string itemId)
        {
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid userId);

            return Ok(await _basketService.DeleteBasketItem(userId, itemId));
        }

        [HttpDelete()]
        [Route("Delete/{userId}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(Guid userId)
        {
            await _basketService.DeleteBasket(userId);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddBasketItem([FromBody] ShoppingCartItem item)
        {
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid userId);

            await _basketService.AddItemToBasket(userId, item);

            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // Now that we are passing scopes from the API Gateway we can extract this info from the Claims object
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid userId);

            var response = await _basketService.CheckoutBasket(basketCheckout, userId);

            if (response.Success)
                return Accepted();

            _logger.LogError(response.ErrorMessage);
            return BadRequest();
        }
    }
}
