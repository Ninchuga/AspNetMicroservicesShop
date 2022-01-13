using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Exceptions;
using Ordering.Application.Helpers;
using Ordering.Application.HubConfiguration;
using Ordering.Application.Models.Responses;
using Ordering.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands
{
    public class CancelOrderCommand : IRequest<CancelOrderCommandResponse>
    {
        public CancelOrderCommand(Guid orderId, Guid userId, Guid correlationId)
        {
            OrderId = orderId;
            UserId = userId;
            CorrelationId = correlationId;
        }

        public Guid OrderId { get; }
        public Guid UserId { get; }
        public Guid CorrelationId { get; }
    }

    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderFailedToBeBilledCommandHandler> _logger;
        private readonly IHubContext<OrderStatusHub> _orderStatusHub;
        private readonly IPublishEndpoint _publishEndpoint;

        public CancelOrderCommandHandler(IOrderRepository orderRepository, ILogger<OrderFailedToBeBilledCommandHandler> logger, 
            IHubContext<OrderStatusHub> orderStatusHub, IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _orderStatusHub = orderStatusHub;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<CancelOrderCommandResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
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
                order.SetCanceledOrderStatusAndTime();

                await _orderRepository.SaveChanges();

                await PublishOrderCanceledEvent(request.CorrelationId, order);
            }
            catch (Exception ex)
            {
                _logger.LogError("Order {OrderId} was not canceled. The reason {ErrorMessage}.", request.OrderId, ex.Message);
                return new CancelOrderCommandResponse(success: false, ex.Message);
            }

            _logger.LogInformation("Order id {OrderId} canceled.", request.OrderId);
            _logger.LogInformation("Notifying the clients about the status change...");

            await _orderStatusHub.Clients.All.SendAsync("OrderStatusUpdated", order.Id, order.OrderStatus.GetDescription());

            return new CancelOrderCommandResponse(success: true, errorMessage: string.Empty);
        }

        private async Task PublishOrderCanceledEvent(Guid correlationId, Order order)
        {
            var orderCanceled = new OrderCanceled
            {
                CorrelationId = correlationId,
                CustomerUsername = order.UserName,
                OrderCreationDate = order.OrderDate,
                OrderId = order.Id,
                OrderTotalPrice = order.TotalPrice,
                PaymentCardNumber = order.PaymentData.CardNumber,
                OrderCancellationDate = order.OrderCancellationDate
            };

            await _publishEndpoint.Publish(orderCanceled);
        }
    }
}
