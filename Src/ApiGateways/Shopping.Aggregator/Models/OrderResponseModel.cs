﻿using System;

namespace Shopping.Aggregator.Models
{
    public class OrderResponseModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }

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
    }
}
