using Destructurama.Attributed;
using System;
using System.Collections.Generic;

namespace Shopping.Razor.Models
{
    public class UserOrder
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public bool OrderPaid { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? OrderCancellationDate { get; set; }

        // BillingAddress
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        // Payment
        [NotLogged]
        public string CardName { get; set; }
        [LogMasked(ShowLast = 4)] // show only 4 last digits
        public string CardNumber { get; set; }
        [NotLogged]
        public string CardExpiration { get; set; }
        [NotLogged]
        public int CVV { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
