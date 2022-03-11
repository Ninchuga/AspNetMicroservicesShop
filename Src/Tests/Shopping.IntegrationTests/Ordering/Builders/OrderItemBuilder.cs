using Ordering.Application.Models;
using System;

namespace Shopping.IntegrationTests.Ordering.Builders
{
    public class OrderItemBuilder
    {
        private static string ProductName => "iPhone X";
        private static int Quantity => 1;
        private static decimal Price => 350;
        private static decimal Discount => 20;

        public static OrderItemDto BuildDto()
        {
            return new OrderItemDto()
            {
                ProductId = GenerateProductId(),
                ProductName = ProductName,
                Discount = Discount,
                Price = Price,
                Quantity = Quantity
            };
        }

        public static OrderItemDto BuildWithProductName(string productName)
        {
            return new OrderItemDto
            {
                ProductId = GenerateProductId(),
                ProductName = productName,
                Discount = Discount,
                Price = Price,
                Quantity = Quantity
            };
        }

        public static OrderItemDto BuildWithProductId(string productId)
        {
            return new OrderItemDto
            {
                ProductId = productId,
                ProductName = ProductName,
                Discount = Discount,
                Price = Price,
                Quantity = Quantity
            };
        }

        public static OrderItemDto BuildWithQuantityAndPrice(int quantity, decimal price)
        {
            return new OrderItemDto
            {
                ProductId = GenerateProductId(),
                ProductName = ProductName,
                Discount = Discount,
                Price = price,
                Quantity = quantity
            };
        }

        public static OrderItemDto BuildWithQuantityAndPriceAndDiscount(int quantity, decimal price, decimal discount)
        {
            return new OrderItemDto
            {
                ProductId = GenerateProductId(),
                ProductName = ProductName,
                Discount = discount,
                Price = price,
                Quantity = quantity
            };
        }

        private static string GenerateProductId()
        {
            Random rnd = new Random();
            int num = rnd.Next(1000000);

            return num.ToString();
        }
    }
}
