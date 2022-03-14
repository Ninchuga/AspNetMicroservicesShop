using Basket.API.Entities;
using System;

namespace Shopping.IntegrationTests.Basket.Builders
{
    public class ShoppingBasketItemBuilder
    {
        public static int Quantity => 1;
        public static string Color => string.Empty;
        public static decimal Price => 300;
        public static string ProductId => "1233";
        public static string ProductName => "iPhone X";
        public static decimal Discount => 20;
        public static decimal PriceWithDiscount => Price - Discount;

        public static ShoppingBasketItem Build()
        {
            return new ShoppingBasketItem(Quantity, Color, Price, GenerateProductId(), ProductName, Discount);
        }

        public static ShoppingBasketItem BuildWith(string productName)
        {
            return new ShoppingBasketItem(Quantity, Color, Price, GenerateProductId(), productName, Discount);
        }

        public static ShoppingBasketItem BuildWith(int quantity, decimal price)
        {
            return new ShoppingBasketItem(quantity, Color, price, GenerateProductId(), ProductName, Discount);
        }

        public static ShoppingBasketItem BuildWith(string productName, int quantity, decimal price, decimal discount)
        {
            return new ShoppingBasketItem(quantity, Color, price, GenerateProductId(), productName, discount);
        }

        public static ShoppingBasketItem BuildWith(string productId, string productName, int quantity, decimal price, decimal discount)
        {
            return new ShoppingBasketItem(quantity, Color, price, productId, productName, discount);
        }

        private static string GenerateProductId()
        {
            Random rnd = new Random();
            int num = rnd.Next(1000000);

            return num.ToString();
        }
    }
}
