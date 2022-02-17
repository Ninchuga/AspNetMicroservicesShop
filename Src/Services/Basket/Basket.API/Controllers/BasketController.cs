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
        private static int _numberOfRequests = 0;

        public BasketController(ILogger<BasketController> logger, IBasketService basketService)
        {
            _logger = logger;
            _basketService = basketService;
        }

        [HttpGet("{userId}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingBasket>> GetBasket(Guid userId)
        {
            // UserId passed from the API Gateway through request headers
            // There token was taken from sub claim of the authenticated user
            //Guid usId = Guid.Parse(HttpContext.Request.Headers["CurrentUser"][0]);

            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid usrId);

            _logger.LogInformation("Retrieving basket for the user {UserId}", usrId);

            return Ok(await _basketService.GetBasketBy(userId));

            //_numberOfRequests++;
            //if (_numberOfRequests % 4 == 0) // every forth request will be successfull
            //{
            //    Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid usrId);

            //    _logger.LogInformation("Retrieving basket for the user {UserId}", usrId);

            //    return Ok(await _basketService.GetBasketBy(userId));
            //}
            //else
            //{
            //    await Task.Delay(TimeSpan.FromSeconds(11)); // for testing purposes

            //    return BadRequest();
            //    //return Ok(await _basketService.GetBasketBy(userId));
            //}
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ShoppingBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingBasket>> UpdateBasket([FromBody] ShoppingBasket basket)
        {
            return Ok(await _basketService.UpdateBasket(basket));
        }

        [HttpDelete()]
        [Route("[action]/{itemId}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingBasket>> DeleteBasketItem(string itemId)
        {
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid userId);

            var response = await _basketService.DeleteBasketItem(userId, itemId);

            _logger.LogInformation("Basket item with id: {ItemId} deleted.", itemId);

            return Ok(response);
        }

        [HttpDelete()]
        [Route("Delete/{userId}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(Guid userId)
        {
            await _basketService.DeleteBasket(userId);

            _logger.LogInformation("Basket for the user with id: {UserId} deleted.", userId);

            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddBasketItem([FromBody] ShoppingBasketItem item)
        {
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid userId);

            await _basketService.AddItemToBasket(userId, item);

            _logger.LogInformation("Item {ItemName} added to basket.", item.ProductName);

            return Ok();
        }
    }
}
