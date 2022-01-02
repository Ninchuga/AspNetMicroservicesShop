using Shopping.Razor.Models;
using System.Collections.Generic;

namespace Shopping.Razor.Responses.Order
{
    public class UserOrdersResponse : BaseResponse
    {
        public IReadOnlyList<UserOrder> UserOrder { get; set; }
    }
}
