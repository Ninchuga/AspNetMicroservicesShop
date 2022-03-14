using Basket.API.Entities;
using Basket.API.Repositories;
using Basket.API.Services.Basket;
using Basket.API.Services.Discount;
using Discount.Grpc.Protos;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shopping.IntegrationTests.Basket.Builders;
using Shopping.IntegrationTests.Utility.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.IntegrationTests.Basket.Services
{
    [Trait("Basket", "Service Tests")]
    public class BasketServiceTests : IClassFixture<BasketFixture>
    {
        private readonly BasketFixture _fixture;
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<BasketService> _logger;
        private Mock<IDiscountService> _discountServiceMock;

        public BasketServiceTests(BasketFixture fixture)
        {
            _fixture = fixture;
            _basketRepository = _fixture.GetBasketRepository();
            _logger = _fixture.GetBasketServiceLogger();
            //_fixture.ResetRedisCache(); // tests randomly fail when flushing enabled
            _discountServiceMock = new Mock<IDiscountService>();
        }

        [Fact]
        public async Task ShouldInsertUserShoppingBasketSuccessfully()
        {
            Guid userId = Guid.NewGuid();
            var basketItem = ShoppingBasketItemBuilder.Build();
            var basket = ShoppingBasketBuilder.BuildWithBasketItem(userId, basketItem);
            SetupDiscountService(basketItem.ProductName, basketItem.ProductName, basketItem.ProductId, (int)basketItem.Discount);

            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            var insertedBasket = await basketService.UpsertBasket(basket);

            insertedBasket.Should().NotBeNull();
            insertedBasket.Items.Should().NotBeNullOrEmpty();
            insertedBasket.UserId.Should().Be(basket.UserId);

            RemoveKey(basket.UserId);
        }

        [Fact]
        public async Task ShouldGetUserBasketAndUpdateBasketItemsDiscountWhenThereIsDelta()
        {
            Guid userId = Guid.NewGuid();
            int discountAmount = 20;
            var basketItem = ShoppingBasketItemBuilder.BuildWith(productName: "iPhone X", quantity: 1, price: 450, discount: 10);
            var basket = ShoppingBasketBuilder.BuildWithBasketItem(userId, basketItem);
            SetupDiscountService(basketItem.ProductName, basketItem.ProductName, basketItem.ProductId, discountAmount);
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            ShoppingBasket insertedBasket = await basketService.UpsertBasket(basket);
            
            var userBasket = await basketService.GetUserBasketAndCheckForItemsDiscount(basket.UserId);

            var item = userBasket.Items.First(item => item.ProductId.Equals(basketItem.ProductId));
            item.Discount.Should().Be(discountAmount);
            item.ProductName.Should().Be(basketItem.ProductName);
            item.Price.Should().Be(basketItem.Price);
            item.PriceWithDiscount.Should().Be(basketItem.Price - discountAmount);

            RemoveKey(basket.UserId);
        }

        [Fact]
        public async Task ShouldGetUserBasketAndNotUpdateBasketItemsDiscountWhenThereIsNoDiscountForThatItem()
        {
            Guid userId = Guid.NewGuid();
            int discountAmount = 20;
            var basketItem = ShoppingBasketItemBuilder.BuildWith(productName: "iPhone X", quantity: 1, price: 450, discount: 10);
            var basket = ShoppingBasketBuilder.BuildWithBasketItem(userId, basketItem);
            SetupDiscountService(basketItem.ProductName, "Samsung Galaxy X", basketItem.ProductId, discountAmount);
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            await basketService.UpsertBasket(basket);

            var userBasket = await basketService.GetUserBasketAndCheckForItemsDiscount(basket.UserId);

            var item = userBasket.Items.First(item => item.ProductId.Equals(basketItem.ProductId));
            item.Discount.Should().Be(basketItem.Discount);
            item.ProductName.Should().Be(basketItem.ProductName);
            item.Price.Should().Be(basketItem.Price);
            item.PriceWithDiscount.Should().Be(basketItem.PriceWithDiscount);

            RemoveKey(basket.UserId);
        }

        [Fact]
        public async Task ShouldGetEmptyUserBasketWhenBasketDoesntExistsForCurrentUser()
        {
            Guid userId = Guid.NewGuid();
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);

            var userBasket = await basketService.GetUserBasketAndCheckForItemsDiscount(userId);

            userBasket.Should().NotBeNull();
            userBasket.UserId.Should().NotBeEmpty().And.Be(userId);
            userBasket.Items.Should().BeEmpty();

            RemoveKey(userId);
        }

        [Fact]
        public async Task ShouldAddBasketItemToUserBasket()
        {
            Guid userId = Guid.NewGuid();
            var basketItem = ShoppingBasketItemBuilder.BuildWith(productName: "iPhone X", quantity: 1, price: 450, discount: 10);

            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            await basketService.AddItemToBasket(userId, basketItem);

            var repository = _fixture.GetBasketRepository();
            var userBasket = await repository.GetBasket(userId);
            userBasket.Should().NotBeNull();
            userBasket.UserId.Should().Be(userId);
            userBasket.Items.Should().NotBeNullOrEmpty();
            userBasket.Items.Should().HaveCount(1);
            var userBasketItem = userBasket.Items.First();
            userBasketItem.Quantity.Should().Be(basketItem.Quantity);
            userBasketItem.Discount.Should().Be(basketItem.Discount);
            userBasketItem.Price.Should().Be(basketItem.Price);
            userBasketItem.ProductId.Should().Be(basketItem.ProductId);
            userBasketItem.PriceWithDiscount.Should().Be(basketItem.Price - basketItem.Discount);

            RemoveKey(userId);
        }

        [Theory]
        [InlineData("555333", "iPhone X", 1, 450, 10)]
        public async Task ShouldUpdateQuantityAndPriceForTheGivenExistingBasketItem(string productId, string productName, int quantity, decimal price, decimal discount)
        {
            Guid userId = Guid.NewGuid();
            var exisitngBasketItem = ShoppingBasketItemBuilder.BuildWith(productId, productName, quantity, price, discount);
            var newBasketItemToAdd = ShoppingBasketItemBuilder.BuildWith(productId, productName, quantity, price, discount);
            var basket = ShoppingBasketBuilder.BuildWithBasketItem(userId, exisitngBasketItem);
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            await basketService.UpsertBasket(basket);

            await basketService.AddItemToBasket(userId, newBasketItemToAdd);

            var repository = _fixture.GetBasketRepository();
            var userBasket = await repository.GetBasket(userId);
            userBasket.Should().NotBeNull();
            userBasket.UserId.Should().Be(userId);
            userBasket.Items.Should().NotBeNullOrEmpty();
            userBasket.Items.Should().HaveCount(1);
            var userBasketItem = userBasket.Items.First();
            userBasketItem.Quantity.Should().Be(2);
            userBasketItem.Discount.Should().Be(exisitngBasketItem.Discount);
            userBasketItem.Price.Should().Be(900);
            userBasketItem.ProductId.Should().Be(exisitngBasketItem.ProductId);
            userBasketItem.PriceWithDiscount.Should().Be(900 - discount);

            RemoveKey(userId);
        }

        [Theory]
        [InlineData("111", "222", "Samsung Galaxy X", 350, 10)]
        public async Task ShouldDeleteBasketItemWhenItemExists(string product1Id, string product2Id, string product2Name, decimal product2Price, decimal product2Discount)
        {
            Guid userId = Guid.NewGuid();
            var basketItem1 = ShoppingBasketItemBuilder.BuildWith(product1Id, productName: "iPhone X", quantity: 1, price: 450, discount: 20);
            var basketItem2 = ShoppingBasketItemBuilder.BuildWith(product2Id, product2Name, quantity: 1, product2Price, product2Discount);
            var basketItems = new List<ShoppingBasketItem> { basketItem1, basketItem2 };
            var basket = ShoppingBasketBuilder.BuildWithMultipleBasketItems(userId, basketItems);
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            await basketService.UpsertBasket(basket);

            var userBasket = await basketService.DeleteBasketItem(userId, product1Id);

            userBasket.Should().NotBeNull();
            userBasket.UserId.Should().Be(userId);
            userBasket.Items.Should().NotBeNullOrEmpty();
            userBasket.Items.Should().HaveCount(1);
            var userBasketItem = userBasket.Items.First();
            userBasketItem.Quantity.Should().Be(1);
            userBasketItem.Discount.Should().Be(product2Discount);
            userBasketItem.Price.Should().Be(product2Price);
            userBasketItem.ProductId.Should().Be(product2Id);
            userBasketItem.PriceWithDiscount.Should().Be(product2Price - product2Discount);

            RemoveKey(userId);
        }

        [Theory]
        [InlineData("888888")]
        [InlineData("999999")]
        public async Task ShouldNotDeleteBasketItemWhenItemDoesntExist(string nonExistingProductId)
        {
            Guid userId = Guid.NewGuid();
            var basketItem = ShoppingBasketItemBuilder.BuildWith(productId: "111111", productName: "iPhone X", quantity: 1, price: 450, discount: 10);
            var basket = ShoppingBasketBuilder.BuildWithBasketItem(userId, basketItem);
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            await basketService.UpsertBasket(basket);

            var userBasket = await basketService.DeleteBasketItem(userId, nonExistingProductId);

            userBasket.Should().NotBeNull();
            userBasket.UserId.Should().Be(userId);
            userBasket.Items.Should().NotBeNullOrEmpty();
            userBasket.Items.Should().HaveCount(1);
            var userBasketItem = userBasket.Items.First();
            userBasketItem.Quantity.Should().Be(1);
            userBasketItem.Discount.Should().Be(basketItem.Discount);
            userBasketItem.Price.Should().Be(basketItem.Price);
            userBasketItem.ProductId.Should().Be(basketItem.ProductId);
            userBasketItem.PriceWithDiscount.Should().Be(basketItem.Price - basketItem.Discount);

            RemoveKey(userId);
        }

        [Fact]
        public async Task ShouldDeleteExistingUserBasket()
        {
            Guid userId = Guid.NewGuid();
            var basketItem = ShoppingBasketItemBuilder.BuildWith(productId: "111111", productName: "iPhone X", quantity: 1, price: 450, discount: 10);
            var basket = ShoppingBasketBuilder.BuildWithBasketItem(userId, basketItem);
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);
            await basketService.UpsertBasket(basket);

            await basketService.DeleteBasket(userId);

            var repository = _fixture.GetBasketRepository();
            var userBasket = await repository.GetBasket(userId);
            userBasket.Should().BeNull();
        }

        [Fact]
        public async Task ShouldNotFailWhenDeletingNonExistingUserBasket()
        {
            Guid userId = Guid.NewGuid();
            var basketService = new BasketService(_discountServiceMock.Object, _basketRepository, _logger);

            await basketService.DeleteBasket(userId);

            var repository = _fixture.GetBasketRepository();
            var userBasket = await repository.GetBasket(userId);
            userBasket.Should().BeNull();
        }


        private void RemoveKey(Guid userId)
        {
            var cache = _fixture.GetDistributedCache();
            cache.Remove(userId.ToString());
        }

        private void SetupDiscountService(string productNameToFind, string couponProductName, string productId, int discountAmount)
        {
            _discountServiceMock.Setup(x => x.GetDiscount(productNameToFind))
                .ReturnsAsync((string providedProductName) =>
                {
                    var couponModel = new CouponModel
                    {
                        Id = int.Parse(productId),
                        ProductName = couponProductName,
                        Amount = discountAmount,
                        Description = string.Empty
                    };

                    if (providedProductName == couponProductName)
                        return couponModel;
                    else
                        return null;
                });
        }
    }
}
