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

        public BasketService(IMapper mapper, IPublishEndpoint publishEndpoint,
            ITokenExchangeServiceFactory tokenExchangeServiceFactory, DiscountGrpcService discountGrpcService, IBasketRepository basketRepository)
        {
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _tokenExchangeServiceFactory = tokenExchangeServiceFactory;
            _discountGrpcService = discountGrpcService;
            _basketRepository = basketRepository;
        }

        public async Task<ShoppingCart> GetBasketBy(Guid userId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            return basket ?? new ShoppingCart(userId);
        }

        public async Task<CheckoutBasketResponse> CheckoutBasket(BasketCheckout basketCheckout, Guid userId)
        {
            var basket = await _basketRepository.GetBasket(userId);
            if (basket == null)
                return new CheckoutBasketResponse { Success = false, ErrorMessage = $"Basket for userId: {userId} doesn't exist." };

            foreach (var item in basket.Items)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            var tokenExchangeService = _tokenExchangeServiceFactory.GetTokenExchangeServiceInstance(DownstreamServices.OrderApi);

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.UserId = userId;
            eventMessage.SecurityContext.AccessToken = await tokenExchangeService.GetAccessTokenForDownstreamService();
            eventMessage.TotalPrice = basket.TotalPrice;
            await _publishEndpoint.Publish(eventMessage);

            await _basketRepository.DeleteBasket(userId);

            return new CheckoutBasketResponse { Success = true };
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            // TODO: place in here logic for updating the basket instead on the client app
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
