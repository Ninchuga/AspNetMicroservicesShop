﻿using FluentAssertions;
using Ordering.Application.Features.Orders.Queries;
using Ordering.Domain.Common;
using Shopping.IntegrationTests.Ordering.Builders;
using Shopping.IntegrationTests.Utility.Ordering;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.IntegrationTests.Ordering.Queries
{
    [Collection("Ordering collection")]
    public class GetOrderQueryTests : IAsyncLifetime
    {
        private readonly OrderingFixture _fixture;

        public GetOrderQueryTests(OrderingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldReturnRequestedOrder()
        {
            var orderItemDto = OrderItemBuilder.BuildDto();
            var placeOrderCommand = PlaceOrderCommandBuilder.BuildWithOrderItem(orderItemDto);
            var placeOrderResponse = await _fixture.Send(placeOrderCommand);

            var getOrderQuery = new GetOrderQuery(placeOrderResponse.OrderId, Guid.NewGuid());
            var order = await _fixture.Send(getOrderQuery);

            order.OrderId.Should().Be(placeOrderResponse.OrderId);
            order.OrderStatus.Should().Be(OrderStatus.PENDING.ToString());
            order.OrderPaid.Should().Be(false);
            order.OrderItems.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldReturnNullWhenOrderDoesntExist()
        {
            Guid nonExistingOrderId = Guid.NewGuid();
            var getOrderQuery = new GetOrderQuery(nonExistingOrderId, Guid.NewGuid());
            var order = await _fixture.Send(getOrderQuery);

            order.Should().BeNull();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => _fixture.ResetDbState();
    }
}
