using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shopping.OrderSagaOrchestrator.Extensions;
using Shopping.OrderSagaOrchestrator.Models;
using Shopping.OrderSagaOrchestrator.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderSagaController : ControllerBase
    {
        private readonly ILogger<OrderSagaController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderSagaController(ILogger<OrderSagaController> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        // for testing purposes
        [HttpGet]
        public async Task<IEnumerable<OrderDto>> GetAllOrders()
        {
            return new List<OrderDto>();
        }

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult> ExecuteOrderTransaction([FromBody] OrderDto orderDto)
        {
            var correlationId = new Guid(HttpContext.Request.Headers[Correlation.Constants.Headers.CorrelationIdHeader][0]);

            var orderPlacedEvent = new OrderPlaced
            {
                OrderId = orderDto.OrderId,
                CorrelationId = correlationId,
                OrderCreationDateTime = orderDto.OrderPlaced,
                PaymentCardNumber = orderDto.PaymentCardNumber,
                OrderTotalPrice = orderDto.OrderTotalPrice
            };
            await _publishEndpoint.Publish(orderPlacedEvent);

            return Ok();
        }
    }
}
