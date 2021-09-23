using Basket.API.Constants;
using Basket.API.Factories;
using Discount.Grpc.Protos;
using Grpc.Core;
using System.Threading.Tasks;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;
        private readonly ITokenExchangeServiceFactory _tokenExchangeServiceFactory;

        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService, ITokenExchangeServiceFactory tokenExchangeServiceFactory)
        {
            _discountProtoService = discountProtoService;
            _tokenExchangeServiceFactory = tokenExchangeServiceFactory;
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };

            var headers = new Metadata();
            var tokenExchangeService = _tokenExchangeServiceFactory.GetTokenExchangeServiceInstance(DownstreamServices.DiscountApi);
            var token = await tokenExchangeService.GetAccessTokenForDownstreamService();
            headers.Add("Authorization", $"Bearer {token}");

            // In the .NET gRPC client, the token can be sent with calls by using the Metadata collection.\
            // Entries in the Metadata collection are sent with a gRPC call as HTTP headers:
            return await _discountProtoService.GetDiscountAsync(discountRequest, headers);
        }
    }
}
