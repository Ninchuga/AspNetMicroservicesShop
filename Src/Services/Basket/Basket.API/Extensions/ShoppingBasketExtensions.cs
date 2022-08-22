using Basket.API.DTOs;
using Basket.API.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Basket.API.Extensions
{
    public static class ShoppingBasketExtensions
    {
        public static ShoppingBasketDto ToDto(this ShoppingBasket basket) =>
            new()
            {
                UserId = basket.UserId,
                TotalPrice = basket.TotalPrice,
                Items = basket.Items.ToDtoList()
            };

        public static ShoppingBasketItemDto ToDto(this ShoppingBasketItem item) =>
            new()
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Discount = item.Discount,
                Price = item.Price,
                Quantity = item.Quantity,
                PriceWithDiscount = item.PriceWithDiscount
            };
        public static ShoppingBasketItem ToDomain(this ShoppingBasketItemDto item) =>
            new(item.Quantity, item.Color, item.Price, item.ProductId, item.ProductName, item.Discount);

        public static List<ShoppingBasketItemDto> ToDtoList(this IEnumerable<ShoppingBasketItem> items) =>
            items.Select(item => item.ToDto()).ToList();
    }
}
