using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Features.Orders.Queries;
using Ordering.Application.Models;
using Shopping.Correlation.Constants;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public OrderController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("GetOrders/{userId}", Name = "GetOrders")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersFor(Guid userId)
        {
            var query = new GetOrdersListQuery(userId);
            var orders = await _mediator.Send(query);

            return Ok(orders);
        }

        [HttpGet("GetOrder/{orderId}", Name = "GetOrder")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<OrderDto>> GetOrderFor(Guid orderId)
        {
            var query = new GetOrderQuery(orderId);
            var order = await _mediator.Send(query);

            return Ok(order);
        }

        [HttpDelete("{orderId}", Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteOrder(Guid orderId)
        {
            Guid correlationId = new(HttpContext.Request.Headers[Headers.CorrelationIdHeader][0]);
            var command = new DeleteOrderCommand(orderId, correlationId);
            
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderDto order)
        {
            var command = _mapper.Map<PlaceOrderCommand>(order);
            command.CorrelationId = new Guid(HttpContext.Request.Headers[Headers.CorrelationIdHeader][0]);

            var response = await _mediator.Send(command);
            return response.Success ? Ok() : BadRequest();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> CancelOrder([FromBody] CancelOrderDto order)
        {
            Guid correlationId = new(HttpContext.Request.Headers[Headers.CorrelationIdHeader][0]);
            var command = new CancelOrderCommand(order.OrderId, correlationId);

            var response = await _mediator.Send(command);
            return response.Success ? Ok() : BadRequest();
        }
    }
}
