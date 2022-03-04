using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using Ordering.Domain.ValueObjects;
using System;

namespace Shopping.UnitTests.Ordering.Builders
{
    public class OrderBuilder
    {
        private static Guid OrderId => Guid.NewGuid();
        private static Guid UserId => Guid.NewGuid();
        private static string UserName => "Test_Username";
        private static decimal TotalPrice => 100;
        private static OrderStatus OrderStatus => OrderStatus.PENDING;
        private static DateTime OrderDate => DateTime.UtcNow;
        private static Address Address => new Address("Test", "User", Email.From("someemail@hotmail.com"), "Pasterova 24", "SRB", "NS");
        private static PaymentData PaymentData => PaymentData.From("Test User", "123456", false, CVV.From(555));

        public static Order BuildWithoutOrderItem()
        {
            return new Order(OrderId, UserId, UserName, TotalPrice, OrderStatus, OrderDate, Address, PaymentData);
        }

        public static Order BuildWithOrderItem(OrderItem orderItem)
        {
            var order = new Order(OrderId, UserId, UserName, TotalPrice, OrderStatus, OrderDate, Address, PaymentData);
            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);

            return order;
        }

        public static Order BuildOrderWithStatus(OrderStatus orderStatus, OrderItem orderItem)
        {
            var order = new Order(OrderId, UserId, UserName, TotalPrice, orderStatus, OrderDate, Address, PaymentData);
            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);

            return order;
        }

        public static Order BuildOrderWithOrderDate(DateTime orderDate, OrderItem orderItem)
        {
            var order = new Order(OrderId, UserId, UserName, TotalPrice, OrderStatus, orderDate, Address, PaymentData);
            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);

            return order;
        }

        public static Order BuildOrderWithOrderDateAndStatus(DateTime orderDate, OrderStatus orderStatus, OrderItem orderItem)
        {
            var order = new Order(OrderId, UserId, UserName, TotalPrice, orderStatus, orderDate, Address, PaymentData);
            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);

            return order;
        }
    }
}
