﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Razor.Models
{
    public class BasketItem
    {
        public string ProductId { get; init; }

        public string ProductName { get; init; }

        public decimal Price { get; init; }

        public decimal Discount { get; init; }

        public int Quantity { get; init; }

        public decimal PriceWithDiscount { get; set; }
    }
}