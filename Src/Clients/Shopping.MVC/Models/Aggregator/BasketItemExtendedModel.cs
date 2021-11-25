﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.MVC.Models.Aggregator
{
    public class BasketItemExtendedModel
    {
        public int Quantity { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Discount { get; set; }

        //Product Related Additional Fields
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string ImageFile { get; set; }
    }
}
