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
    public class OrderPaidCommandTests : IAsyncLifetime
    {
        private readonly OrderingFixture _fixture;

        public OrderPaidCommandTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldSetOrderToPaidSuccessfully()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var orderPaidCommand = new OrderPaidCommand(placeOrderResponse.OrderId, Guid.NewGuid());
            await _fixture.Send(orderPaidCommand);

            var order = await _fixture.GetOrderBy(placeOrderResponse.OrderId);
            order.Should().NotBeNull();
            order.OrderStatus.Should().Be(OrderStatus.ORDER_PAID);
            order.PaymentData.OrderPaid.Should().BeTrue();
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionWhenThereIsNoOrderForUpdateToPaid()
        {
            var nonExistingOrderId = Guid.NewGuid();
            var orderPaidCommand = new OrderPaidCommand(nonExistingOrderId, Guid.NewGuid());

            FluentActions.Invoking(() => _fixture.Send(orderPaidCommand))
                .Should().ThrowAsync<NotFoundException>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => _fixture.ResetDbState();
    }
}
