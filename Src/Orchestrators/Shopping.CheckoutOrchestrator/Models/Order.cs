using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.OrderSagaOrchestrator.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime OrderPlaced { get; set; }
        public OrderStatus OrderState { get; set; }
    }
}
