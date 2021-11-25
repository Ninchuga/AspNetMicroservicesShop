using AutoMapper;
using Destructurama.Attributed;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands
{
    public class CheckoutOrderCommand : IRequest<CheckoutOrderCommandResponse>
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }

        // BillingAddress
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }

        // Payment
        [NotLogged]
        public string CardName { get; set; }
        [LogMasked(ShowLast = 4)]
        public string CardNumber { get; set; }
    }

    public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, CheckoutOrderCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckoutOrderCommandHandler> _logger;

        public CheckoutOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService, ILogger<CheckoutOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<CheckoutOrderCommandResponse> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId} {UserId}", request.CorrelationId, request.UserId);
            _logger.LogInformation("Creating order {@Order}", request);

            var orderEntity = _mapper.Map<Order>(request);
            orderEntity.Id = Guid.NewGuid();
            orderEntity.OrderPaid = false;
            orderEntity.OrderPlaced = DateTime.UtcNow;

            try
            {
                var newOrder = await _orderRepository.AddAsync(orderEntity);

                _logger.LogInformation("Order {OrderId} successfully created.", newOrder.Id);

                //await SendMail(newOrder);

                return new CheckoutOrderCommandResponse(success: true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order for the user {UserId} failed to create.", request.UserId);

                return new CheckoutOrderCommandResponse(success: false, ex.Message);
            }
        }

        private async Task SendMail(Order order)
        {
            var email = new Email() { To = "ninoslav90@hotmail.com", Body = $"Order was created.", Subject = "Order was created" };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Order {order.Id} failed due to an error with the mail service: {ex.Message}");
            }
        }
    }
}
