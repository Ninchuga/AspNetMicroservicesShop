using System;
using System.Collections.Generic;

namespace Shopping.Razor.Models
{
    public class ShoppingBasket
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public List<ShoppingBasketItem> Items { get; set; } = new List<ShoppingBasketItem>();

        public decimal TotalPrice { get; set; }
    }
}
