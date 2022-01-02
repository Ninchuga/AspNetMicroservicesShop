using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Razor.Models
{
    public class BasketItem
    {
        public int Quantity { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Discount { get; set; }
    }
}
