using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Entities
{
    public class ShoppingBasket
    {
        public Guid UserId { get; set; }
        public List<ShoppingBasketItem> Items { get; set; } = new List<ShoppingBasketItem>();

        public ShoppingBasket()
        {
        }

        public ShoppingBasket(Guid userId)
        {
            UserId = userId;
        }

        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    totalPrice += item.Price * item.Quantity;
                }

                return totalPrice;
            }
        }
    }
}
