using AutoMapper;
using Destructurama.Attributed;
using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Application.Models.Responses;
using Ordering.Application.Services;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using Ordering.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands
{
    public class PlaceOrderCommand : IRequest<OrderPlacedCommandResponse>
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }

        // BillingAddress
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        // Payment
        [NotLogged]
        public string CardName { get; set; }
        [LogMasked(ShowLast = 4)]
        public string CardNumber { get; set; }

        [NotLogged]
        public string CardExpiration { get; set; }
        [NotLogged]
        public int CVV { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderPlacedCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ITokenExchangeService _tokenExchangeService;
        private readonly IConfiguration _configuration;

        public PlaceOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService,
            ILogger<PlaceOrderCommandHandler> logger, IPublishEndpoint publishEndpoint, ITokenExchangeService tokenExchangeService, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _tokenExchangeService = tokenExchangeService;
            _configuration = configuration;
        }

        public async Task<OrderPlacedCommandResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId} {UserId}", request.CorrelationId, request.UserId);
            _logger.LogInformation("Creating order {@Order}", request);

            var order = new Order(
                orderId: Guid.NewGuid(),
                request.UserId,
                request.UserName,
                request.TotalPrice,
                OrderStatus.PENDING,
                orderDate: DateTime.UtcNow,
                address: new Address(request.FirstName, request.LastName, request.Email, request.Street, request.Country, request.City),
                paymentData: new PaymentData(request.CardName, request.CardNumber, orderPaid: false, request.CVV)
                );

            foreach (var item in request.OrderItems)
            {
                order.AddOrderItem(item.ProductId, item.ProductName, item.Price, item.Discount, item.Quantity);
            }

            try
            {
                await _orderRepository.Add(order);
                bool orderInserted = await _orderRepository.SaveChanges();
                if (orderInserted)
                {
                    _logger.LogInformation("Order {OrderId} successfully created.", order.Id);

                    _logger.LogInformation("Sending email for the created order {OrderId}", order.Id);

                    await _emailService.SendMailFor(request.Email, request.UserName, order.Id);

                    _logger.LogInformation("Publishing {EventName} event...", nameof(OrderPlaced));

                    await PublishOrderPlacedEvent(order, request.CorrelationId);

                    // TODO: publish message to basket service queue to delete the basket items or
                    // trigger Azure Function when order placed to delete the user basket
                }

                return orderInserted 
                    ? new OrderPlacedCommandResponse(success: orderInserted, errorMessage: string.Empty)
                    : new OrderPlacedCommandResponse(success: orderInserted, errorMessage: "Order failed to be inserted in db.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order for the user {UserId} failed to be created.", request.UserId);

                return new OrderPlacedCommandResponse(success: false, ex.Message);
            }
        }

        private async Task PublishOrderPlacedEvent(Order order, Guid correlationId)
        {
            string accessToken = await _tokenExchangeService.ExchangeAccessToken(
                tokenExchangeCacheKey: _configuration["DownstreamServicesTokenExhangeCacheKeys:OrderSagaOrchestrator"],
                serviceScopes: _configuration["DownstreamServicesScopes:OrderSagaOrchestrator"]);

            var orderPlacedEvent = new OrderPlaced
            {
                OrderId = order.Id,
                CorrelationId = correlationId,
                OrderCreationDate = order.OrderDate,
                PaymentCardNumber = order.PaymentData.CardNumber,
                OrderTotalPrice = order.TotalPrice,
                CustomerUsername = order.UserName
            };
            orderPlacedEvent.SecurityContext.AccessToken = accessToken;
            
            await _publishEndpoint.Publish(orderPlacedEvent);
        }
    }
}
