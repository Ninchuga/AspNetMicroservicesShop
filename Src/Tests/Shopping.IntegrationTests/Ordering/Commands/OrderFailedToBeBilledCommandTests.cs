using FluentAssertions;
using Ordering.Application.Exceptions;
using Ordering.Application.Features.Orders.Commands;
using Ordering.Domain.Common;
using Shopping.IntegrationTests.Ordering.Builders;
using Shopping.IntegrationTests.Utility.Ordering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.IntegrationTests.Ordering.Commands
{
    [Collection("Ordering collection")]
    public class OrderFailedToBeBilledCommandTests : IClassFixture<OrderingFixture>
    {
        private readonly OrderingFixture _fixture;

        public OrderFailedToBeBilledCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldSetOrderStatusToPendingWhenOrderFailedToBeBilled()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var orderFailedToBeBilledCommand = new OrderFailedToBeBilledCommand(placeOrderResponse.OrderId, Guid.NewGuid());
            await _fixture.Send(orderFailedToBeBilledCommand);

            var order = await _fixture.GetOrderBy(placeOrderResponse.OrderId);
            order.Should().NotBeNull();
            order.OrderStatus.Should().Be(OrderStatus.PENDING);
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionWhenThereIsNoOrderToSetToDispatched()
        {
            var nonExistingOrderId = Guid.NewGuid();
            var orderFailedToBeBilledCommand = new OrderFailedToBeBilledCommand(nonExistingOrderId, Guid.NewGuid());

            FluentActions.Invoking(() => _fixture.Send(orderFailedToBeBilledCommand))
                .Should().ThrowAsync<NotFoundException>();
        }
    }
}
