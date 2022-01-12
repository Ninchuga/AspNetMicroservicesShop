using Shopping.Razor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Razor.Extensions
{
    public static class BasketExtensions
    {
        public static OrderItem ToOrderItem(this BasketItem basketItem)
        {
            return new OrderItem
            {
                ProductId = basketItem.ProductId,
                ProductName = basketItem.ProductName,
                Price = basketItem.Price,
                Quantity = basketItem.Quantity,
                Discount = basketItem.Discount
            };
        }

        public static List<OrderItem> ToOrderItems(this IEnumerable<BasketItem> basketItems) =>
            basketItems.Select(item => item.ToOrderItem()).ToList();

        public static UserOrder ToOrderWithItemsFrom(this BasketCheckout checkout, IEnumerable<BasketItem> basketItems)
        {
            return new UserOrder
            {
                UserId = checkout.UserId,
                UserName = checkout.UserName,
                TotalPrice = checkout.TotalPrice,
                FirstName = checkout.FirstName,
                LastName = checkout.LastName,
                Email = checkout.Email,
                Street = checkout.Street,
                Country = checkout.Country,
                City = checkout.City,
                CardName = checkout.CardName,
                CardExpiration = checkout.CardExpiration,
                CVV = checkout.CVV,
                OrderItems = basketItems.ToOrderItems()
            };
        }
    }
}
