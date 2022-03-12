using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Queries
{
    public class GetOrdersListQuery : IRequest<List<OrderDto>>
    {
        public Guid UserId { get; }
        public Guid CorrelationId { get; }

        public GetOrdersListQuery(Guid userId, Guid correlationId)
        {
            UserId = userId;
            CorrelationId = correlationId;
        }
    }

    public class GetOrdersListQueryHandler : IRequestHandler<GetOrdersListQuery, List<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrdersListQueryHandler> _logger;

        public GetOrdersListQueryHandler(IOrderRepository orderRepository, IMapper mapper, ILogger<GetOrdersListQueryHandler> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<OrderDto>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId} {UserId}", request.CorrelationId, request.UserId);

            var orders = await _orderRepository.GetOrdersBy(request.UserId);
            if (orders == null || !orders.Any())
                _logger.LogWarning("There are no orders for the specified user id {UserId}", request.UserId);

            return _mapper.Map<List<OrderDto>>(orders);
        }
    }
}
