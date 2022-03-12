using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Exceptions;
using Ordering.Application.Helpers;
using Ordering.Application.HubConfiguration;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands
{
    public class OrderDeliveredCommand : IRequest
    {
        public OrderDeliveredCommand(Guid orderId, Guid correlationId)
        {
            OrderId = orderId;
            CorrelationId = correlationId;
        }

        public Guid OrderId { get; }
        public Guid CorrelationId { get; }
    }

    public class OrderDeliveredCommandHandler : IRequestHandler<OrderDeliveredCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderDeliveredCommandHandler> _logger;
        private readonly IHubContext<OrderStatusHub> _orderStatusHub;

        public OrderDeliveredCommandHandler(IOrderRepository orderRepository, ILogger<OrderDeliveredCommandHandler> logger, IHubContext<OrderStatusHub> orderStatusHub)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _orderStatusHub = orderStatusHub;
        }

        public async Task<Unit> Handle(OrderDeliveredCommand request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", request.CorrelationId);

            var order = await _orderRepository.GetOrderBy(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("There is no order with id: {OrderId} to update.", request.OrderId);
                throw new NotFoundException(nameof(Order), request.OrderId);
            }

            try
            {
                order.SetOrderStatusToDelivered();

                await _orderRepository.SaveChanges();

                _logger.LogInformation("Order id {OrderId} status updated to {OrderStatus}.", request.OrderId, OrderStatus.ORDER_DELIVERED);

                _logger.LogInformation("Notifying the clients about the status change...");

                await _orderStatusHub.Clients.All.SendAsync("OrderStatusUpdated", order.Id, order.OrderStatus.GetDescription());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error happened while executing the command {Command} for order {OrderId}. The reason: {ErrorMessage}.", nameof(OrderDeliveredCommand), request.OrderId, ex.Message);
            }

            return Unit.Value;
        }
    }
}
