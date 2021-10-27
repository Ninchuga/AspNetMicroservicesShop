using AutoMapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Discount.Grpc.Repositories;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discount.Grpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly IDiscountRepository _repository;
        private readonly ILogger<DiscountService> _logger;
        private readonly IMapper _mapper;

        public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var coupon = await _repository.GetDiscount(request.ProductName);
            if(coupon == null)
            {
                _logger.LogInformation("Discount for product name: {ProductName} was not found.", coupon.ProductName);
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount for product name: {request.ProductName} was not found."));
            }

            var couponModel = _mapper.Map<CouponModel>(coupon);

            _logger.LogInformation("Retrieving coupon discount {@Coupon}", coupon);

            return couponModel;
        }

        public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            var coupon = _mapper.Map<Coupon>(request.Coupon);
            await _repository.CreateDiscount(coupon);
            _logger.LogInformation("Discount is successfully created. ProductName: {ProductName}", coupon.ProductName);

            return request.Coupon;
        }

        public override async Task<CouponModel> UpdateDiscount(UpdateDiscounRequest request, ServerCallContext context)
        {
            var coupon = _mapper.Map<Coupon>(request.Coupon);
            await _repository.UpdateDiscount(coupon);

            _logger.LogInformation("Discount is successfully updated. ProductName: {ProductName}", coupon.ProductName);

            return request.Coupon;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            var deleted = await _repository.DeleteDiscount(request.ProductName);
            _logger.LogInformation("Discount {ProductName} deleted: {deleted}", request.ProductName, deleted);

            var response = new DeleteDiscountResponse
            {
                Success = deleted
            };

            return response;
        }
    }
}
