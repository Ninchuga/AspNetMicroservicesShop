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
    public class OrderDeliveredCommandTests : IClassFixture<OrderingFixture>
    {
        private readonly OrderingFixture _fixture;

        public OrderDeliveredCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldSetOrderStatusToDeliveredSuccessfully()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var orderDeliveredCommand = new OrderDeliveredCommand(placeOrderResponse.OrderId, Guid.NewGuid());
            await _fixture.Send(orderDeliveredCommand);

            var order = await _fixture.GetOrderBy(placeOrderResponse.OrderId);
            order.Should().NotBeNull();
            order.OrderStatus.Should().Be(OrderStatus.ORDER_DELIVERED);
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionWhenThereIsNoOrderToSetToDelivered()
        {
            var nonExistingOrderId = Guid.NewGuid();
            var orderDeliveredCommand = new OrderDeliveredCommand(nonExistingOrderId, Guid.NewGuid());

            FluentActions.Invoking(() => _fixture.Send(orderDeliveredCommand))
                .Should().ThrowAsync<NotFoundException>();
        }
    }
}
