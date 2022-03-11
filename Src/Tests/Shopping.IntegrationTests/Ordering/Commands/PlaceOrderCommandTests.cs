using FluentAssertions;
using Shopping.IntegrationTests.Ordering.Builders;
using Shopping.IntegrationTests.Utility.Ordering;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.IntegrationTests.Ordering.Commands
{
    [Collection("Ordering collection")]
    public class PlaceOrderCommandTests : IClassFixture<OrderingFixture>
    {
        private readonly OrderingFixture _fixture;

        public PlaceOrderCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetDbState().GetAwaiter().GetResult();
        }

        [Fact]
        [Trait("Ordering", "Place Order Successfully")]
        public async Task ShouldPlaceAnOrderSuccessfully()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var command = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);

            var response = await _fixture.Send(command);

            response.Success.Should().BeTrue();
            response.ErrorMessage.Should().BeEmpty();
        }

        [Fact]
        [Trait("Ordering", "Place Order With Invalid Email")]
        public void PlaceOrderCommandHnadlerShouldThrowExceptionWhenUserEmailIsNotValid()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var command = PlaceOrderCommandBuilder.BuildWithOrderItemAndEmail(orderItemDto, "invalidemailaddress@ggg");

            FluentActions.Invoking(() => _fixture.Send(command))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        [Trait("Ordering", "Place Order Without Order Item")]
        public async Task ShouldReturnUnsuccessfulResponseWhenThereAreNoOrderItemsInsideOrder()
        {
            var command = PlaceOrderCommandBuilder.BuildWithoutOrderItem();

            var response = await _fixture.Send(command);

            response.Success.Should().BeFalse();
            response.ErrorMessage.Should().Be("Order must have at least one order item.");
        }
    }
}
