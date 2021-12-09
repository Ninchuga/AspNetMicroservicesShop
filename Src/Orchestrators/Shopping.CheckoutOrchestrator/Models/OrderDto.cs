using System;

namespace Shopping.OrderSagaOrchestrator.Models
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public DateTime OrderPlaced { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public decimal OrderTotalPrice { get; set; }
        public string PaymentCardNumber { get; set; }
    }
}
