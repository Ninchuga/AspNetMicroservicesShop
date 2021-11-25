using Shopping.CheckoutOrchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Extensions
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
                OrderState = OrderState.PENDING
            };
        }
    }
}
