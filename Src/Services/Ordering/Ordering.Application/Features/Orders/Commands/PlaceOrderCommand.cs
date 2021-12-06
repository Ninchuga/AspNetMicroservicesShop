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
        private readonly OrderPlacedPublisher _orderPlacedPublisher;


        public PlaceOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService,
            ILogger<PlaceOrderCommandHandler> logger, IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
            //_orderPlacedPublisher = orderPlacedPublisher;
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

            try
            {
                await _orderRepository.AddAsync(order);

                _logger.LogInformation("Order {OrderId} successfully created.", order.Id);

                //await SendMail(newOrder);

                //var response = await CallOrderSaga(orderEntity);

                await PublishOrderPlacedEvent(order, request.CorrelationId);

                //var orderPlacedEvent = new OrderPlaced
                //{
                //    OrderId = order.Id,
                //    CorrelationId = request.CorrelationId,
                //    OrderCreationDateTime = order.OrderPlaced,
                //    PaymentCardNumber = order.CardNumber,
                //    OrderTotalPrice = order.TotalPrice,
                //    CustomerUsername = order.UserName
                //};

                //await _orderPlacedPublisher.SendMessage(orderPlacedEvent);

                // publish message to basket service queue to delete the basket items

                //return new OrderPlacedCommandResponse(success: response.IsSuccessStatusCode, errorMessage: response.ReasonPhrase);
                return new OrderPlacedCommandResponse(success: true, errorMessage: string.Empty);
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

        private async Task<HttpResponseMessage> CallOrderSaga(Order order)
        {
            var httpClient = _httpClientFactory.CreateClient("OrderSaga");
            var orderSagaRequest = new OrderSaga(order.Id, order.OrderPlaced, order.OrderStatus);
            var requestContent = new StringContent(JsonSerializer.Serialize(orderSagaRequest), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync("OrderSaga/ExecuteOrderTransaction", requestContent);

            return response;
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

        private class OrderSaga
        {
            public OrderSaga(Guid orderId, DateTime orderPlaced, OrderStatus orderStatus)
            {
                OrderId = orderId;
                OrderPlaced = orderPlaced;
                OrderStatus = orderStatus;
            }

            public Guid OrderId { get;  }
            public DateTime OrderPlaced { get; }
            public OrderStatus OrderStatus { get; }
        }
    }
}
