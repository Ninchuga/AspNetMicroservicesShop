using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Models
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public DateTime OrderPlaced { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public decimal OrderTotalPrice { get; set; }
    }
}
