using Shopping.Razor.Models;
using System.Collections.Generic;
using System.Linq;

namespace Shopping.Razor.Extensions
{
    public static class BasketExtensions
    {
        public static OrderItem ToOrderItem(this ShoppingBasketItem basketItem)
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

        public static List<OrderItem> ToOrderItems(this IEnumerable<ShoppingBasketItem> basketItems) =>
            basketItems.Select(item => item.ToOrderItem()).ToList();

        public static UserOrder ToOrderWithItemsFrom(this BasketCheckout checkout, IEnumerable<ShoppingBasketItem> basketItems)
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
                CardNumber = checkout.CardNumber,
                CardExpiration = checkout.CardExpiration,
                CVV = checkout.CVV,
                OrderItems = basketItems.ToOrderItems()
            };
        }
    }
}
