using System.Collections.Generic;

namespace Shopping.Aggregator.Models
{
    public class BasketModel
    {
        public string UserName { get; set; } = string.Empty;
        public IReadOnlyCollection<BasketItemExtendedModel> Items { get; set; } = new List<BasketItemExtendedModel>();
        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    totalPrice += item.PriceWithDiscount;
                }

                return totalPrice;
            }
        }
    }
}
