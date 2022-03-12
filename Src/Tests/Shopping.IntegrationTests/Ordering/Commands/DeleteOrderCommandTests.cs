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
    public class DeleteOrderCommandTests : IClassFixture<OrderingFixture>
    {
        private readonly OrderingFixture _fixture;

        public DeleteOrderCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldDeleteOrderSuccessfully()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var deleteOrderCommand = new DeleteOrderCommand(placeOrderResponse.OrderId, Guid.NewGuid());
            await _fixture.Send(deleteOrderCommand);

            var order = await _fixture.GetOrderBy(placeOrderResponse.OrderId);
            order.Should().BeNull();
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionWhenThereIsNoOrderToDelete()
        {
            var nonExistingOrderId = Guid.NewGuid();
            var deleteOrderCommand = new DeleteOrderCommand(nonExistingOrderId, Guid.NewGuid());

            FluentActions.Invoking(() => _fixture.Send(deleteOrderCommand))
                .Should().ThrowAsync<NotFoundException>();
        }
    }
}
