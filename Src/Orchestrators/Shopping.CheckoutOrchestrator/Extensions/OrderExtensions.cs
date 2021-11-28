using Shopping.OrderSagaOrchestrator.Models;
using System;

namespace Shopping.OrderSagaOrchestrator.Extensions
{
    public static class OrderExtensions
    {
        public static Order ToEntityWith(this OrderDto orderDto, Guid correlationId)
        {
            return new Order
            {
                OrderId = orderDto.OrderId,
                CorrelationId = correlationId,
                OrderPlaced = orderDto.OrderPlaced,
                OrderState = OrderStatus.PENDING
            };
        }

        public static Order ToEntityWith(this OrderDto orderDto)
        {
            return new Order
            {
                OrderId = orderDto.OrderId,
                CorrelationId = Guid.NewGuid(),
                OrderPlaced = orderDto.OrderPlaced,
                OrderState = OrderStatus.PENDING
            };
        }
    }
}
