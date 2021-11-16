using System.Collections.Generic;

namespace Shopping.Aggregator.Models
{
    public class ShoppingModel
    {
        public BasketModel BasketWithProducts { get; set; }
        public IReadOnlyCollection<OrderResponseModel> Orders { get; set; } = new List<OrderResponseModel>();
    }
}
