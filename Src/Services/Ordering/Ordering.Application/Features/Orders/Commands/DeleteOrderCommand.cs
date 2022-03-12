using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Exceptions;
using Ordering.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands
{
    public class DeleteOrderCommand : IRequest
    {
        public DeleteOrderCommand(Guid orderId, Guid correlationId)
        {
            OrderId = orderId;
            CorrelationId = correlationId;
        }

        public Guid OrderId { get; }
        public Guid CorrelationId { get; }
    }

    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<DeleteOrderCommandHandler> _logger;

        public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<DeleteOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var orderToDelete = await _orderRepository.GetOrderBy(request.OrderId);
            if (orderToDelete == null)
            {
                _logger.LogWarning("There is no order with id: {OrderId} to update.", request.OrderId);
                throw new NotFoundException(nameof(Order), request.OrderId);
            }

            try
            {
                _orderRepository.Delete(orderToDelete);
                await _orderRepository.SaveChanges();
                _logger.LogInformation("Order {OrderId} successfully deleted.", orderToDelete.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order {OrderId} failed to be deleted with message {Message}.", orderToDelete.Id, ex.Message);
            }

            return Unit.Value;
        }
    }
}
