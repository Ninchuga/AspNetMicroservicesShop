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
    public class OrderDispatchedCommandTests : IClassFixture<OrderingFixture>
    {
        private readonly OrderingFixture _fixture;

        public OrderDispatchedCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldSetOrderStatusToDispatchedSuccessfully()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var orderDispatchedCommand = new OrderDispatchedCommand(placeOrderResponse.OrderId, Guid.NewGuid());
            await _fixture.Send(orderDispatchedCommand);

            var order = await _fixture.GetOrderBy(placeOrderResponse.OrderId);
            order.Should().NotBeNull();
            order.OrderStatus.Should().Be(OrderStatus.ORDER_DISPATCHED);
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionWhenThereIsNoOrderToSetToDispatched()
        {
            var nonExistingOrderId = Guid.NewGuid();
            var orderDispatchedCommand = new OrderDispatchedCommand(nonExistingOrderId, Guid.NewGuid());

            FluentActions.Invoking(() => _fixture.Send(orderDispatchedCommand))
                .Should().ThrowAsync<NotFoundException>();
        }
    }
}
