using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.MVC.Models
{
    public class BasketWithItems
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    //totalPrice += item.Price * item.Quantity;
                    totalPrice += item.Price;
                }

                return totalPrice;
            }
        }
    }
}
