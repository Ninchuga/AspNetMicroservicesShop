using MediatR;
using Ordering.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Features.Orders.Commands
{
    public class UpdateOrderStatusCommand : IRequest
    {
        public Guid OrderId { get; set; }
        public Guid CorrelationId { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
