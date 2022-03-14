using Basket.API.Entities;
using System;
using System.Collections.Generic;

namespace Shopping.IntegrationTests.Basket.Builders
{
    public class ShoppingBasketBuilder
    {
        public static ShoppingBasket BuildWithBasketItem(Guid userId, ShoppingBasketItem basketItem)
        {
            var basket = new ShoppingBasket(userId);
            basket.AddBasketItem(basketItem);

            return basket;
        }

        public static ShoppingBasket BuildWithMultipleBasketItems(Guid userId, List<ShoppingBasketItem> basketItems)
        {
            var basket = new ShoppingBasket(userId);
            foreach (var item in basketItems)
            {
                basket.AddBasketItem(item);
            }

            return basket;
        }

        public static ShoppingBasket BuildEmptyBasket(Guid userId)
        {
            return new ShoppingBasket(userId);
        }
    }
}
