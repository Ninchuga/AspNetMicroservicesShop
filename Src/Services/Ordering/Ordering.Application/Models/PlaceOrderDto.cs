using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Models
{
    public class PlaceOrderDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }

        // BillingAddress
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }

        // Payment
        public string CardName { get; set; }
        public string CardNumber { get; set; }
    }
}
