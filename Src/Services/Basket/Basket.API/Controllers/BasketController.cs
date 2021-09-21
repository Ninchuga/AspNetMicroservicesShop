using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _discountGrpcService = discountGrpcService;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
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

            var basket = await _repository.GetBasket(userId);
            return Ok(basket ?? new ShoppingCart(userId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            foreach (var item in basket.Items)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }
            
            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(Guid userId)
        {
            await _repository.DeleteBasket(userId);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // we take user id from the header because it's more safer than fetching it from the basketCheckout dto
            //Guid userId = Guid.Parse(HttpContext.Request.Headers["CurrentUser"][0]);

            // Now that we are passing scopes from the API Gateway we can extract this info from the Claims object
            Guid.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value, out Guid userId);

            var basket = await _repository.GetBasket(userId);
            if (basket == null)
                return BadRequest();

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;
            await _publishEndpoint.Publish(eventMessage);

            await _repository.DeleteBasket(userId);

            return Accepted();
        }
    }
}
