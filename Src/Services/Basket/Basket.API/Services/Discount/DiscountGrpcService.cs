using Discount.Grpc.Protos;
using System.Threading.Tasks;

namespace Basket.API.Services.Discount
{
    public class DiscountGrpcService : IDiscountService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
        {
            _discountProtoService = discountProtoService;
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };
            
            return await _discountProtoService.GetDiscountAsync(discountRequest);
        }
    }
}
