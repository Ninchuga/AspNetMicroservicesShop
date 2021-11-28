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
        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return new List<Order>();
            //return await _store.GetOrders();
        }

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult> ExecuteOrderTransaction([FromBody] OrderDto orderDto)
        {
            var correlationId = new Guid(HttpContext.Request.Headers[Correlation.Constants.Headers.CorrelationIdHeader][0]);
            var order = orderDto.ToEntityWith(correlationId);

            var orderPlacedEvent = new OrderPlaced
            {
                OrderId = order.OrderId,
                CorrelationId = order.CorrelationId,
                OrderCreationDateTime = order.OrderPlaced,
                PaymentCardNumber = "123456",
                OrderTotalPrice = orderDto.OrderTotalPrice
            };
            await _publishEndpoint.Publish(orderPlacedEvent); // publish to payment service

            return Ok();
        }
    }
}
