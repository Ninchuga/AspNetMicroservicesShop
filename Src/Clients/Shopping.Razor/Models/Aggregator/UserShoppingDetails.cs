using Shopping.Razor.Responses;
using System.Collections.Generic;

namespace Shopping.Razor.Models.Aggregator
{
    public class UserShoppingDetails : BaseResponse
    {
        public BasketModel BasketWithProducts { get; set; }
        public IReadOnlyList<UserOrder> Orders { get; set; } = new List<UserOrder>();
    }
}
