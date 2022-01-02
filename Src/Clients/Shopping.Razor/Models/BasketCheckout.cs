using Destructurama.Attributed;
using System;

namespace Shopping.Razor.Models
{
    public class BasketCheckout
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
        public string City { get; set; }

        // Payment
        [NotLogged] // with this attribute we say Serilog not to log this property
        public string CardName { get; set; }
        [LogMasked(ShowLast = 4)] // show only 4 last digits
        public string CardNumber { get; set; }
        [NotLogged]
        public string CardExpiration { get; set; }
        [NotLogged]
        public string CVV { get; set; }
    }
}
