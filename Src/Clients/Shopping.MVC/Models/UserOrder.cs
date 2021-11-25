using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace Shopping.MVC.Models
{
    public class UserOrder
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }

        // BillingAddress
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }

        // Payment
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public DateTime OrderPlaced { get; set; }
        public bool OrderPaid { get; set; }

        public string OrderStatusToDisplay(OrderStatus orderStatus) =>
            orderStatus switch
            {
                OrderStatus.PENDING => "Pending",
                OrderStatus.ORDER_BILLED => "Order Billed",
                OrderStatus.ORDER_DISPATCHED => "Order Displatched",
                OrderStatus.ORDER_DELIVERED => "Order Delivered",
                _ => string.Empty,
            };
    }

    public enum OrderStatus
    {
        PENDING,
        ORDER_BILLED,
        ORDER_DISPATCHED,
        ORDER_DELIVERED
    }
}
