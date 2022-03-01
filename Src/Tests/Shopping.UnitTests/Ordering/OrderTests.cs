using FluentAssertions;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using Ordering.Domain.Exceptions;
using Shopping.UnitTests.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.UnitTests.Ordering
{
    public class OrderTests
    {
        [Theory]
        [InlineData("123", "iPhone X", 330, 20, 1)]
        [InlineData("124", "Samsung X", 303, 20, 1)]
        public void ShouldAddOneOrderItemToTheOrderSuccessfully(string productId, string productName, decimal itemPrice, decimal discount, int quantity)
        {
            var order = OrderBuilder.BuildWithoutOrderItem();

            order.AddOrderItem(productId, productName, itemPrice, discount, quantity);

            order.OrderItems.Should().NotBeEmpty();
            order.OrderItems.Should().HaveCount(1);
        }

        [Theory]
        [MemberData(nameof(TestOrderDataGenerator.GetOrderItems), MemberType = typeof(TestOrderDataGenerator))]
        public void ShouldAddThreeOrderItemsToTheOrderSuccessfully(OrderItem orderItem1, OrderItem orderItem2, OrderItem orderItem3)
        {
            var order = OrderBuilder.BuildWithoutOrderItem();

            order.AddOrderItem(orderItem1.ProductId, orderItem1.ProductName, orderItem1.Price, orderItem1.Discount, orderItem1.Quantity);
            order.AddOrderItem(orderItem2.ProductId, orderItem2.ProductName, orderItem2.Price, orderItem2.Discount, orderItem2.Quantity);
            order.AddOrderItem(orderItem3.ProductId, orderItem3.ProductName, orderItem3.Price, orderItem3.Discount, orderItem3.Quantity);

            order.OrderItems.Should().NotBeEmpty();
            order.OrderItems.Should().HaveCount(3);
        }

        [Fact]
        public void ShouldNotAddOrderItemToTheOrderwhenTheOrderItemWithTheSameProductIdAlreadyExists()
        {
            var order = OrderBuilder.BuildWithoutOrderItem();
            var orderItem = OrderItemBuilder.Build();

            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);
            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);

            order.OrderItems.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldUpdateOrderItemQuantityWithProvidedValueWhenOrderItemWithTheSameProductIdAlreadyExists()
        {
            var order = OrderBuilder.BuildWithoutOrderItem();
            var orderItem = OrderItemBuilder.Build();

            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, orderItem.Quantity);
            order.AddOrderItem(orderItem.ProductId, orderItem.ProductName, orderItem.Price, orderItem.Discount, 5);

            order.OrderItems.Should().HaveCount(1);
            OrderItem existingOrderItem = order.OrderItems.First(item => item.ProductId.Equals(orderItem.ProductId));
            existingOrderItem.Quantity.Should().Be(6);
        }

        [Fact]
        public void WhenOrderPaymentIsFinishedItShouldUpdateOrderStatusToPaid()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildWithOrderItem(orderItem);

            order.SetOrderToPaid();

            order.OrderStatus.Should().Be(OrderStatus.ORDER_PAID);
            order.PaymentData.OrderPaid.Should().BeTrue();
        }

        [Fact]
        public void ShouldCancelOrderWhenItsPlacedInThe24HoursTimeAndInPendingState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildWithOrderItem(orderItem);

            order.SetCanceledOrderStatusAndTime();

            order.OrderStatus.Should().Be(OrderStatus.ORDER_CANCELED);
            order.OrderCancellationDate.Should().NotBeNull();
        }

        [Fact]
        public void ShouldCancelOrderWhenItsPlacedInThe24HoursTimeAndInFailedToBeBilledState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithStatus(OrderStatus.ORDER_FAILED_TO_BE_BILLED, orderItem);

            order.SetCanceledOrderStatusAndTime();

            order.OrderStatus.Should().Be(OrderStatus.ORDER_CANCELED);
            order.OrderCancellationDate.Should().NotBeNull();
        }

        [Fact]
        public void ShouldNotCancelOrderWhenItsPlacedInThe24HoursTimeAndInOrderPaidState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithStatus(OrderStatus.ORDER_PAID, orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("Order is already billed and can't be canceled.");
        }

        [Fact]
        public void ShouldNotCancelOrderWhenItsPlacedInThe24HoursTimeAndInOrderDispatchedState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithStatus(OrderStatus.ORDER_DISPATCHED, orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("Order is already billed and can't be canceled.");
        }

        [Fact]
        public void ShouldNotCancelOrderWhenItsPlacedInThe24HoursTimeAndInOrderDeliveredState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithStatus(OrderStatus.ORDER_DELIVERED, orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("Order is already billed and can't be canceled.");
        }

        [Fact]
        public void ShouldNotCancelOrderWhenItsPlacedInThe24HoursTimeAndInOrderCanceledState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithStatus(OrderStatus.ORDER_CANCELED, orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("Order is already canceled.");
        }

        [Fact]
        public void ShouldNotCancelOrderWhenOrderIsPlacedMoreThan24HoursAndInPendingState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithOrderDate(DateTime.UtcNow.AddHours(-48), orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("You can only cancel your order in the first 24h when the order was placed.");
        }

        [Fact]
        public void ShouldNotCancelOrderWhenOrderIsPlacedMoreThan24HoursAndInFailedToBeBilledState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithOrderDate(DateTime.UtcNow.AddHours(-48), orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("You can only cancel your order in the first 24h when the order was placed.");
        }

        [Fact]
        public void ShouldNotCancelOrderWhenOrderIsPlacedMoreThan24HoursAndInPaidState()
        {
            var orderItem = OrderItemBuilder.Build();
            var order = OrderBuilder.BuildOrderWithOrderDateAndStatus(DateTime.UtcNow.AddHours(-48), OrderStatus.ORDER_PAID, orderItem);

            order.Invoking(order => order.SetCanceledOrderStatusAndTime())
                .Should().Throw<OrderCancelationException>()
                .WithMessage("Order is already billed and can't be canceled.");
        }
    }
}
