using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Models
{
    public class OrderItemDto
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        public int Quantity { get; set; }
    }
}
