using Discount.Grpc.Protos;
using System.Threading.Tasks;

namespace Basket.API.Services.Discount
{
    public interface IDiscountService
    {
        Task<CouponModel> GetDiscount(string productName);
    }
}
