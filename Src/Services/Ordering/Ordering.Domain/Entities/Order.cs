using Ordering.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Entities
{
    public class Order : EntityBase
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
        public DateTime OrderPlaced { get; set; }
        public bool OrderPaid { get; set; }

        //public string Expiration { get; set; }
        //public string CVV { get; set; }
    }
}
