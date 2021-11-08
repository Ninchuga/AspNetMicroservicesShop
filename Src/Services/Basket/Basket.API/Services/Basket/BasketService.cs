using AutoMapper;
using Basket.API.Constants;
using Basket.API.DTOs;
using Basket.API.Entities;
using Basket.API.Factories;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Basket.API.Responses;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ITokenExchangeServiceFactory _tokenExchangeServiceFactory;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<BasketService> _logger;

        public BasketService(IMapper mapper, IPublishEndpoint publishEndpoint,
            ITokenExchangeServiceFactory tokenExchangeServiceFactory, DiscountGrpcService discountGrpcService, 
            IBasketRepository basketRepository, ILogger<BasketService> logger)
        {
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _tokenExchangeServiceFactory = tokenExchangeServiceFactory;
            _discountGrpcService = discountGrpcService;
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task<ShoppingCart> GetBasketBy(Guid userId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            return basket ?? new ShoppingCart(userId);
        }

        public async Task<CheckoutBasketResponse> CheckoutBasket(BasketCheckout basketCheckout, Guid userId, string correlationId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            if (basket == null)
            {
                _logger.LogInformation("There is no basket for user id: {UserId}", userId);
                return new CheckoutBasketResponse { Success = false, ErrorMessage = $"Basket for userId: {userId} doesn't exist." };
            }

            foreach (var item in basket.Items)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            try
            {
                var tokenExchangeService = _tokenExchangeServiceFactory.GetTokenExchangeServiceInstance(DownstreamServices.OrderApi);

                var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
                eventMessage.UserId = userId;
                eventMessage.SecurityContext.AccessToken = await tokenExchangeService.GetAccessTokenForDownstreamService();
                eventMessage.TotalPrice = basket.TotalPrice;
                eventMessage.CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid() : new Guid(correlationId);
                await _publishEndpoint.Publish(eventMessage);

                _logger.LogInformation("Message successfully published from {MethodName} to order api for further processing.", nameof(CheckoutBasket));

                await _basketRepository.DeleteBasket(userId);

                _logger.LogInformation("Basket for the user id: {UserId} deleted after checkout.", userId);

                return new CheckoutBasketResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking out basket.");

                return new CheckoutBasketResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            return await _basketRepository.UpdateBasket(basket);
        }

        public async Task DeleteBasket(Guid userId) =>
            await _basketRepository.DeleteBasket(userId);

        public async Task<ShoppingCart> DeleteBasketItem(Guid userId, string itemId)
        {
            var basket = await GetBasketBy(userId);
            var itemToDelete = basket.Items.FirstOrDefault(item => item.ProductId.Equals(itemId));
            basket.Items.Remove(itemToDelete);

            return await UpdateBasket(basket);
        }

        public async Task AddItemToBasket(Guid userId, ShoppingCartItem item)
        {
            var userBasket = await _basketRepository.GetBasket(userId);
            if (userBasket == null)
                userBasket = new ShoppingCart(userId);

            ShoppingCartItem existingItem = userBasket.Items.FirstOrDefault(existing => existing.ProductId.Equals(item.ProductId));
            if(existingItem == null)
            {
                userBasket.Items.Add(item);
            }
            else
            {
                existingItem.Quantity += item.Quantity;
                existingItem.Price += item.Price * item.Quantity;
            }

            await UpdateBasket(userBasket);
        }
    }
}
