﻿using MediatR;
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
    public class OrderDispatchedCommand : IRequest
    {
        public OrderDispatchedCommand(Guid orderId, Guid correlationId)
        {
            OrderId = orderId;
            CorrelationId = correlationId;
        }

        public Guid OrderId { get; }
        public Guid CorrelationId { get; }
    }

    public class OrderDispatchedCommandHandler : IRequestHandler<OrderDispatchedCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderDispatchedCommandHandler> _logger;
        private readonly IHubContext<OrderStatusHub> _orderStatusHub;

        public OrderDispatchedCommandHandler(IOrderRepository orderRepository, ILogger<OrderDispatchedCommandHandler> logger, IHubContext<OrderStatusHub> orderStatusHub)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _orderStatusHub = orderStatusHub;
        }

        public async Task<Unit> Handle(OrderDispatchedCommand request, CancellationToken cancellationToken)
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
                order.SetOrderStatusToDispatched();

                await _orderRepository.SaveChanges();

                _logger.LogInformation("Order id {OrderId} status updated to {OrderStatus}.", request.OrderId, OrderStatus.ORDER_DISPATCHED);

                _logger.LogInformation("Notifying the clients about the status change...");

                await _orderStatusHub.Clients.All.SendAsync("OrderStatusUpdated", order.Id, order.OrderStatus.GetDescription());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error happened while executing the command {Command} for order {OrderId}. The reason: {ErrorMessage}.", nameof(OrderDispatchedCommand), request.OrderId, ex.Message);
            }

            return Unit.Value;
        }
    }
}
