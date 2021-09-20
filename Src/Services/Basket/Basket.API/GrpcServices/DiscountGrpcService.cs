using Basket.API.Helpers;
using Basket.API.Services;
using Discount.Grpc.Protos;
using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;
        private readonly ITokenService _tokenService;
        private readonly GrpcChannelHelper _grpcChannelHelper;
        private readonly IConfiguration _configuration;

        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService, GrpcChannelHelper grpcChannelHelper, IConfiguration configuration, ITokenService tokenService)
        {
            _discountProtoService = discountProtoService;
            _grpcChannelHelper = grpcChannelHelper;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };

            //var channel = await _grpcChannelHelper.CreateAuthorizedChannel(_configuration["GrpcSettings:DiscountUrl"]);
            //var client = new DiscountProtoService.DiscountProtoServiceClient(channel);
            //return await client.GetDiscountAsync(discountRequest);

            var headers = new Metadata();
            var token = await _tokenService.GetAccessTokenForDownstreamServices();
            headers.Add("Authorization", $"Bearer {token}");

            // In the .NET gRPC client, the token can be sent with calls by using the Metadata collection.\
            // Entries in the Metadata collection are sent with a gRPC call as HTTP headers:
            return await _discountProtoService.GetDiscountAsync(discountRequest, headers);
            //return await _discountProtoService.GetDiscountAsync(discountRequest);
        }
    }
}
