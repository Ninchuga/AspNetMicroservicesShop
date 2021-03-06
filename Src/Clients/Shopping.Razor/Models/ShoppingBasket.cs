using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Razor.Models
{
    public class ShoppingBasket
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public List<ShoppingBasketItem> Items { get; set; } = new List<ShoppingBasketItem>();
        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    //totalPrice += item.Price * item.Quantity;
                    totalPrice += item.PriceWithDiscount;
                }

                return totalPrice;
            }
        }
    }
}
