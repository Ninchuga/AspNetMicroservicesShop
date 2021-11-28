using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events.Order
{
    public class OrderStatusUpdated : OrderBaseEvent
    {
        public string OrderStatus { get; set; }
    }
}
