using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events.Order
{
    public class OrderBaseEvent : IntegrationBaseEvent
    {
        public Guid OrderId { get; set; }
        public string CustomerUsername { get; set; }
        public string PaymentCardNumber { get; set; }
        public DateTime? OrderCreationDateTime { get; set; }
        public DateTime? OrderCancelDateTime { get; set; }
        public decimal OrderTotalPrice { get; set; }
    }
}
