using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.CheckoutOrchestrator.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime OrderPlaced { get; set; }
        public OrderState OrderState { get; set; }
    }
}
