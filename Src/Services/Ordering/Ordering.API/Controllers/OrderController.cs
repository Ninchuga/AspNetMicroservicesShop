using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Features.Orders.Queries;
using Ordering.Application.Models;
using Shopping.Correlation.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("{userId}", Name = "GetOrder")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersFor(Guid userId)
        {
            var query = new GetOrdersListQuery(userId);
            var orders = await _mediator.Send(query);

            return Ok(orders);
        }

        [HttpPut]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> UpdateOrder([FromBody] UpdateOrderCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{orderId}", Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteOrder(Guid orderId)
        {
            var command = new DeleteOrderCommand() { OrderId = orderId };
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
            var command = new CancelOrderCommand { OrderId = order.OrderId, UserId = order.UserId };
            command.CorrelationId = new Guid(HttpContext.Request.Headers[Headers.CorrelationIdHeader][0]);

            var response = await _mediator.Send(command);
            return response.Success ? Ok() : BadRequest();
        }
    }
}
