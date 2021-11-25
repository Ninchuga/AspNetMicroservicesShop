using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Models
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public DateTime OrderPlaced { get; set; }
        public OrderState OrderStatus { get; set; }
    }
}
