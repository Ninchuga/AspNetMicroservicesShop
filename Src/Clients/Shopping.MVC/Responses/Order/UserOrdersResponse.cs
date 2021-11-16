using Shopping.MVC.Models;
using System.Collections.Generic;

namespace Shopping.MVC.Responses.Order
{
    public class UserOrdersResponse : BaseResponse
    {
        public IReadOnlyList<UserOrder> UserOrder { get; set; }
    }
}
