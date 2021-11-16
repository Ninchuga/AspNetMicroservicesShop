using Shopping.MVC.Responses;
using System.Collections.Generic;

namespace Shopping.MVC.Models.Aggregator
{
    public class UserShoppingDetails : BaseResponse
    {
        public BasketModel BasketWithProducts { get; set; }
        public IReadOnlyList<UserOrder> Orders { get; set; } = new List<UserOrder>();
    }
}
