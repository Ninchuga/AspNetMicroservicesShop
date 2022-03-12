using FluentAssertions;
using Ordering.Application.Exceptions;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Domain.Common;
using Shopping.IntegrationTests.Ordering.Builders;
using Shopping.IntegrationTests.Utility.Ordering;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.IntegrationTests.Ordering.Commands
{
    [Collection("Ordering collection")]
    public class CancelOrderCommandTests : IClassFixture<OrderingFixture>
    {
        private readonly OrderingFixture _fixture;

        public CancelOrderCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldCancelOrderSuccessfully()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var cancelOrderCommand = new CancelOrderCommand(placeOrderResponse.OrderId, Guid.NewGuid());
            var response = await _fixture.Send(cancelOrderCommand);

            var order = await _fixture.GetOrderBy(placeOrderResponse.OrderId);
            order.Should().NotBeNull();
            order.OrderStatus.Should().Be(OrderStatus.ORDER_CANCELED);
            response.Success.Should().BeTrue();
            response.ErrorMessage.Should().BeEmpty();
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionWhenThereIsNoOrderToCancel()
        {
            var nonExistingOrderId = Guid.NewGuid();
            var cancelOrderCommand = new CancelOrderCommand(nonExistingOrderId, Guid.NewGuid());

            FluentActions.Invoking(() => _fixture.Send(cancelOrderCommand))
                .Should().ThrowAsync<NotFoundException>();
        }
    }
}
