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
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands
{
    public class OrderPaidCommand : IRequest
    {
        public Guid OrderId { get; set; }
        public Guid CorrelationId { get; set; }
    }

    public class OrderPaidCommandHandler : IRequestHandler<OrderPaidCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPaidCommandHandler> _logger;
        private readonly IHubContext<OrderStatusHub> _orderStatusHub;

        public OrderPaidCommandHandler(IOrderRepository orderRepository, ILogger<OrderPaidCommandHandler> logger, IHubContext<OrderStatusHub> orderStatusHub)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _orderStatusHub = orderStatusHub;
        }

        public async Task<Unit> Handle(OrderPaidCommand request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId}", request.CorrelationId);

            var order = await _orderRepository.GetOrderBy(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("There is no order with id: {OrderId} to update.", request.OrderId);
                throw new NotFoundException(nameof(Order), request.OrderId);
            }

            order.SetOrderStatusToPaid();

            await _orderRepository.SaveChanges();

            _logger.LogInformation("Order id {OrderId} successfully paid and status updated to {NewOrderStatus}.", request.OrderId, OrderStatus.ORDER_BILLED);

            _logger.LogInformation("Notifying the clients about the status change...");

            await _orderStatusHub.Clients.All.SendAsync("OrderStatusUpdated", order.Id, order.OrderStatus.GetDescription());

            return Unit.Value;
        }
    }
}
