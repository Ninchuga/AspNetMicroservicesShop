using System;
using System.Collections.Generic;

namespace Basket.API.DTOs
{
    public class ShoppingBasketDto
    {
        public Guid UserId { get; set; }
        public IEnumerable<ShoppingBasketItemDto> Items { get; set; } = new List<ShoppingBasketItemDto>();
        public decimal TotalPrice { get; set; }
    }
}
