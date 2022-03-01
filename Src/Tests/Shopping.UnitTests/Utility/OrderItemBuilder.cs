﻿using Ordering.Domain.Entities;
using System;

namespace Shopping.UnitTests.Utility
{
    public class OrderItemBuilder
    {
        private static string ProductName => "iPhone X";
        private static int Quantity => 1;
        private static decimal Price => 350;
        private static decimal Discount => 20;

        public static OrderItem Build()
        {
            return new OrderItem(GenerateProductId(), Quantity, Price, ProductName, Discount);
        }

        public static OrderItem BuildWithProductName(string productName)
        {
            return new OrderItem(GenerateProductId(), Quantity, Price, productName, Discount);
        }

        public static OrderItem BuildWithProductId(string productId)
        {
            return new OrderItem(productId, Quantity, Price, ProductName, Discount);
        }

        private static string GenerateProductId()
        {
            Random rnd = new Random();
            int num = rnd.Next(1000000);

            return num.ToString();
        }
    }
}
