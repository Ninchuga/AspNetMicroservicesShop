using System;

namespace Delivery.API.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string CustomerUsername { get; set; }
        public DateTime? OrderCreationDateTime { get; set; }
    }
}
