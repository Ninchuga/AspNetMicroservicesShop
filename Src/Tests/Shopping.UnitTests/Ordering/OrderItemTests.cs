using FluentAssertions;
using Ordering.Domain.Exceptions;
using Shopping.UnitTests.Ordering.Builders;
using Xunit;

namespace Shopping.UnitTests.Ordering
{
    public class OrderItemTests
    {
        [Theory]
        [InlineData(5)]
        [InlineData(3.30)]
        [InlineData(9.99)]

        public void ShouldSetNewDiscountToOrderItemWhenDiscountIsGreaterThanZero(decimal discount)
        {
            var orderItem = OrderItemBuilder.BuildWithQuantityAndPrice(3, 500);
            decimal expectedPriceWithDiscount = orderItem.Quantity * (orderItem.Price - discount);

            orderItem.SetNewDiscount(discount);

            orderItem.Discount.Should().Be(discount);
            orderItem.PriceWithDiscount.Should().Be(expectedPriceWithDiscount);
        }

        [Fact]
        public void ShouldSetOrderItemDiscountToZeroWhenZeroIsProvided()
        {
            decimal discount = 0;
            var orderItem = OrderItemBuilder.BuildWithQuantityAndPriceAndDiscount(3, 500, 25);
            decimal expectedPriceWithDiscount = orderItem.Quantity * (orderItem.Price - discount);

            orderItem.SetNewDiscount(discount);

            orderItem.Discount.Should().Be(0);
            orderItem.PriceWithDiscount.Should().Be(expectedPriceWithDiscount);
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(-3.30)]
        [InlineData(-9.99)]

        public void ShouldNotSetNewDiscountToOrderItemWhenDiscountIsLowerThanZero(decimal discount)
        {
            var orderItem = OrderItemBuilder.BuildWithQuantityAndPrice(3, 500);

            orderItem.Invoking(item => item.SetNewDiscount(discount))
                .Should().Throw<OrderingDomainException>()
                .WithMessage("Discount is not valid for the order item");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]

        public void ShouldUpdateOrderItemQuantityWhenQuantityIsGreaterThanZero(int quantity)
        {
            var orderItem = OrderItemBuilder.BuildWithQuantityAndPrice(3, 500);
            int expectedQuantity = orderItem.Quantity + quantity;

            orderItem.UpdateQuantity(quantity);

            orderItem.Quantity.Should().Be(expectedQuantity);
        }

        [Fact]
        public void ShouldNotChangeOrderItemQuantityWhenZeroQuantityIsProvidedForUpdate()
        {
            int initialQuantity = 3;
            int quantityToUpdate = 0;
            var orderItem = OrderItemBuilder.BuildWithQuantityAndPriceAndDiscount(initialQuantity, 500, 25);

            orderItem.UpdateQuantity(quantityToUpdate);

            orderItem.Quantity.Should().Be(initialQuantity);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]

        public void ShouldNotUpdateOrderItemQuantityWhenQuantityIsLowerThanZero(int quantity)
        {
            var orderItem = OrderItemBuilder.BuildWithQuantityAndPrice(3, 500);
            int expectedQuantity = orderItem.Quantity + quantity;

            orderItem.Invoking(item => item.UpdateQuantity(quantity))
                .Should().Throw<OrderingDomainException>()
                .WithMessage("Invalid order item quantity");
        }
    }
}
