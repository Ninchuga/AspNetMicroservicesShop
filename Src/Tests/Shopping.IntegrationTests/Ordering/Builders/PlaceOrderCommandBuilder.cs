using Ordering.Application.Features.Orders.Commands;
using Ordering.Application.Models;
using System;
using System.Collections.Generic;

namespace Shopping.IntegrationTests.Ordering.Builders
{
    public class PlaceOrderCommandBuilder
    {
        private static Guid CorrelationId => Guid.NewGuid();
        private static Guid UserId => Guid.NewGuid();
        private static decimal TotalPrice => 100;
        private static string UserName => "Test_Username";
        private static string FirstName => "Bugz";
        private static string LastName => "Bunny";
        private static string Email => "sometestemail@gmail.com";
        private static string Street => "21st Street";
        private static string Country => "Disneyland";
        private static string City => "DC";
        private static string CardName => "Bugz Bunny";
        private static string CardNumber => "123456778898";
        private static string CardExpiration => DateTime.Now.Month.ToString();
        private static int CVV => 123;
        private static List<OrderItemDto> OrderItems => new List<OrderItemDto>();


        public static PlaceOrderCommand BuildWithOrderItem(OrderItemDto orderItem)
        {
            return new PlaceOrderCommand()
            {
                CorrelationId = CorrelationId,
                UserId = UserId,
                UserName = UserName,
                TotalPrice = TotalPrice,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Street = Street,
                Country = Country,
                City = City,
                CardName = CardName,
                CardNumber = CardNumber,
                CardExpiration = CardExpiration,
                CVV = CVV,
                OrderItems = new List<OrderItemDto> { orderItem }
            };
        }

        public static PlaceOrderCommand BuildWithOrderItemAndEmail(OrderItemDto orderItem, string email)
        {
            return new PlaceOrderCommand()
            {
                CorrelationId = CorrelationId,
                UserId = UserId,
                UserName = UserName,
                TotalPrice = TotalPrice,
                FirstName = FirstName,
                LastName = LastName,
                Email = email,
                Street = Street,
                Country = Country,
                City = City,
                CardName = CardName,
                CardNumber = CardNumber,
                CardExpiration = CardExpiration,
                CVV = CVV,
                OrderItems = new List<OrderItemDto> { orderItem }
            };
        }

        public static PlaceOrderCommand BuildWithoutOrderItem()
        {
            return new PlaceOrderCommand()
            {
                CorrelationId = CorrelationId,
                UserId = UserId,
                UserName = UserName,
                TotalPrice = TotalPrice,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Street = Street,
                Country = Country,
                City = City,
                CardName = CardName,
                CardNumber = CardNumber,
                CardExpiration = CardExpiration,
                CVV = CVV,
                OrderItems = new List<OrderItemDto>()
            };
        }
    }
}
