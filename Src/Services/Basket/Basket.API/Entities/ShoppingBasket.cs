using System;
using System.Collections.Generic;
using System.Linq;

namespace Basket.API.Entities
{
    public class ShoppingBasket
    {
        public ShoppingBasket(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; private set; }
        public List<ShoppingBasketItem> Items { get; private set; } = new List<ShoppingBasketItem>();
        public bool ShouldUpdateBasket { get; private set; }

        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    totalPrice += item.PriceWithDiscount * item.Quantity;
                }

                return totalPrice;
            }
        }

        public void AddBasketItem(ShoppingBasketItem item)
        {
            ShoppingBasketItem existingItem = Items.FirstOrDefault(existing => existing.ProductId.Equals(item.ProductId));
            if (existingItem == null)
            {
                Items.Add(item);
            }
            else
            {
                existingItem.UpdateQuantity(item.Quantity)
                            .UpdatePrice();
            }
        }

        public void UpdateItemDiscount(ShoppingBasketItem item, decimal discount)
        {
            var basketItem = Items.First(bi => bi.ProductId.Equals(item.ProductId));
            if(basketItem.Discount != discount)
            {
                basketItem.UpdateDiscount(discount);
                ShouldUpdateBasket = true;
            }
        }
    }
}
