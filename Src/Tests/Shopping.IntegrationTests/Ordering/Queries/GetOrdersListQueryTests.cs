using FluentAssertions;
using Ordering.Application.Features.Orders.Queries;
using Ordering.Application.Models;
using Shopping.IntegrationTests.Ordering.Builders;
using Shopping.IntegrationTests.Utility.Ordering;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.IntegrationTests.Ordering.Queries
{
    [Collection("Ordering collection")]
    public class GetOrdersListQueryTests : IAsyncLifetime
    {
        private readonly OrderingFixture _fixture;

        public GetOrdersListQueryTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldReturnAllUserOrders()
        {
            Guid userId = Guid.NewGuid();
            Guid firstOrderId = await PlaceOrderForUser(userId, "iPhone X");
            Guid secondOrderId = await PlaceOrderForUser(userId, "Xiaomi Redmi Note X");

            var getOrderQuery = new GetOrdersListQuery(userId, Guid.NewGuid());
            var orders = await _fixture.Send(getOrderQuery);

            orders.Should().NotBeEmpty();
            orders.Should().HaveCount(2);
            var firstOrder = orders.First(order => order.OrderId.Equals(firstOrderId));
            var secondOrder = orders.First(order => order.OrderId.Equals(secondOrderId));
            Assert(firstOrder);
            Assert(secondOrder);
        }

        [Fact]
        public async Task ShouldReturnNullWhenOrdersDontExist()
        {
            Guid nonExistingOrderId = Guid.NewGuid();
            var getOrdersQuery = new GetOrdersListQuery(nonExistingOrderId, Guid.NewGuid());
            var orders = await _fixture.Send(getOrdersQuery);

            orders.Should().BeEmpty();
        }

        private void Assert(OrderDto order)
        {
            order.Email.Should().NotBeNullOrEmpty();
            order.UserId.Should().NotBeEmpty();
            order.OrderStatus.Should().NotBeNullOrEmpty();
            order.OrderItems.Should().NotBeEmpty();
        }

        private async Task<Guid> PlaceOrderForUser(Guid userId, string productName)
        {
            var orderItemDto = OrderItemBuilder.BuildWithProductName(productName);
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItemForUser(orderItemDto, userId);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            return placeOrderResponse.OrderId;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => _fixture.ResetDbState();
    }
}
