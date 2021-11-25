using EventBus.Messages.Checkout;
using EventBus.Messages.Events.Order;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shopping.CheckoutOrchestrator.Extensions;
using Shopping.CheckoutOrchestrator.Models;
using Shopping.CheckoutOrchestrator.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderSagaController : ControllerBase
    {
        private readonly ILogger<OrderSagaController> _logger;
        private readonly IOrderSagaStore _store;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderSagaController(ILogger<OrderSagaController> logger, IOrderSagaStore store, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _store = store;
            _publishEndpoint = publishEndpoint;
        }

        // for testing purposes
        [HttpGet]
        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _store.GetOrders();
        }

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult> ExecuteOrderTransaction([FromBody] OrderDto orderDto)
        {
            var correlationId = new Guid(HttpContext.Request.Headers[Correlation.Constants.Headers.CorrelationIdHeader][0]);

            var order = orderDto.ToEntityWith(correlationId);
            await _store.Insert(order); // store order in pending state

            var orderPlacedEvent = new OrderPlaced();
            //await _publishEndpoint.Publish(orderPlacedEvent); // publish to payment service

            return Ok();

            //var data = JsonSerializer.Serialize(checkout);
            //var checkoutEvent = new Event(Guid.NewGuid(), nameof(Models.Checkout), checkout.UserId, data, DateTime.UtcNow);
            //await _eventStore.SaveChanges(checkoutEvent);

            //await _publishEndpoint.Publish(new ApplyDiscountToUserBasketItems { UserId = checkout.UserId });

            // maybe save checkout into event db 
            // send message received to que and basket service which will find the basket and apply discount from event handle it
            // basket service will than call discount service and apply discout to basket items
            // basket service will send to bus success/fail event
            // if success BasketItemsDiscountAppliedConsumer will find the checkout event in db and send the message to qbasketcheckoutque
            // Order service will handle that message and store the new order
            // if success Order service will send OrderCreated event which will be picked up by OrderCreatedConsumer
            // after that Basket service should consume message and delete basket for the user


            // send BasketCheckoutEvent to que and Order service BasketCheckoutConsumer will handle it
        }

        byte[] SerializeObject(object value) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));

        // Convert a byte array to an Object
        private object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = (object)binForm.Deserialize(memStream);

            return obj;
        }
    }
}
