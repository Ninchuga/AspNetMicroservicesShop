using AutoMapper;
using Destructurama.Attributed;
using EventBus.Messages.Events.Order;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
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

        public PlaceOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService, 
            ILogger<PlaceOrderCommandHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<OrderPlacedCommandResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            using var loggerScope = _logger.BeginScope("{CorrelationId} {UserId}", request.CorrelationId, request.UserId);
            _logger.LogInformation("Creating order {@Order}", request);

            var orderEntity = _mapper.Map<Order>(request);
            orderEntity.Id = Guid.NewGuid();
            orderEntity.OrderPaid = false;
            orderEntity.OrderPlaced = DateTime.UtcNow;
            orderEntity.OrderStatus = OrderStatus.PENDING;

            try
            {
                await _orderRepository.AddAsync(orderEntity);

                _logger.LogInformation("Order {OrderId} successfully created.", orderEntity.Id);

                //await SendMail(newOrder);

                var response = await CallOrderSaga(orderEntity);

                return new OrderPlacedCommandResponse(success: response.IsSuccessStatusCode, errorMessage: response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order for the user {UserId} failed to be created.", request.UserId);

                return new OrderPlacedCommandResponse(success: false, ex.Message);
            }
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
