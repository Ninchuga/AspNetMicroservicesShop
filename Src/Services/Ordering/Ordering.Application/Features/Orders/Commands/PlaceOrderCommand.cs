using AutoMapper;
using Azure.Messaging.ServiceBus;
using Destructurama.Attributed;
using EventBus.Messages.Common;
using EventBus.Messages.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Application.Publishers;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using Shopping.Correlation.Constants;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        public string Address { get; set; }
        public string Country { get; set; }

        // Payment
        [NotLogged]
        public string CardName { get; set; }
        [LogMasked(ShowLast = 4)]
        public string CardNumber { get; set; }
    }

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderPlacedCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPublishEndpoint _publishEndpoint;

        public PlaceOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService,
            ILogger<PlaceOrderCommandHandler> logger, IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<OrderPlacedCommandResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId} {UserId}", request.CorrelationId, request.UserId);
            _logger.LogInformation("Creating order {@Order}", request);

            var order = _mapper.Map<Order>(request);
            order.Id = Guid.NewGuid();
            order.OrderPaid = false;
            order.OrderPlaced = DateTime.UtcNow;
            order.OrderStatus = OrderStatus.PENDING;

            string orderInsertionErrorMessage = string.Empty;

            try
            {
                bool orderInserted = await _orderRepository.AddAsync(order);
                if(orderInserted)
                {
                    _logger.LogInformation("Order {OrderId} successfully created.", order.Id);

                    // TODO: Maybe use Azure Functions to send an email
                    // Image/logo for the email can be retreived from blob storage
                    //await SendMail(newOrder);

                    await PublishOrderPlacedEvent(order, request.CorrelationId);

                    // TODO: publish message to basket service queue to delete the basket items or
                    // trigger Azure Function when order placed to delete the user basket
                }

                return orderInserted 
                    ? new OrderPlacedCommandResponse(success: orderInserted, errorMessage: orderInsertionErrorMessage)
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
            var orderPlacedEvent = new OrderPlaced
            {
                OrderId = order.Id,
                CorrelationId = correlationId,
                OrderCreationDateTime = order.OrderPlaced,
                PaymentCardNumber = order.CardNumber,
                OrderTotalPrice = order.TotalPrice,
                CustomerUsername = order.UserName
            };
            
            await _publishEndpoint.Publish(orderPlacedEvent);
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
