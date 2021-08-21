using Discount.Grpc.Protos;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        private readonly IConfiguration _configuration;
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

        //public DiscountGrpcService(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
        {
            _discountProtoService = discountProtoService;
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            //var channel = GrpcChannel.ForAddress(_configuration["GrpcSettings:DiscountUrl"]);
            //var client = new DiscountProtoService.DiscountProtoServiceClient(channel);

            var discountRequest = new GetDiscountRequest { ProductName = productName };

            return await _discountProtoService.GetDiscountAsync(discountRequest);

            //return await client.GetDiscountAsync(discountRequest);
        }
    }
}
