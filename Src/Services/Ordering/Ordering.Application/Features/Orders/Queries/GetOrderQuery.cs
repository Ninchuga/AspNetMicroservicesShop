using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Queries
{
    public class GetOrderQuery : IRequest<OrderDto>
    {
        public GetOrderQuery(Guid orderId, Guid correlationId)
        {
            OrderId = orderId;
            CorrelationId = correlationId;
        }

        public Guid OrderId { get; }
        public Guid CorrelationId { get; }
    }

    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderQueryHandler> _logger;

        public GetOrderQueryHandler(IOrderRepository orderRepository, IMapper mapper, ILogger<GetOrderQueryHandler> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId} {OrderId}", request.CorrelationId, request.OrderId);

            Order order = await _orderRepository.GetOrderBy(request.OrderId);
            if (order == null)
                _logger.LogWarning("Order with id {OrderId} not found.", request.OrderId);

            return _mapper.Map<OrderDto>(order);
        }
    }
}
