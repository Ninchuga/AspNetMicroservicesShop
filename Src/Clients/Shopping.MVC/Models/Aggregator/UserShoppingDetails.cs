using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.MVC.Models.Aggregator
{
    public class UserShoppingDetails
    {
        public BasketModel BasketWithProducts { get; set; }
        public IEnumerable<UserOrder> Orders { get; set; }
    }
}
