using Ordering.Domain.Common;
using Ordering.Domain.Exceptions;
using System;

namespace Ordering.Domain.Entities
{
    public class OrderItem : BaseEntity<int>
    {
        public OrderItem(string productId, int quantity, decimal price, string productName, decimal discount)
            : base(default)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
            ProductName = productName;
            Discount = discount;
        }

        public string ProductId { get; }
        public string ProductName { get; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }
        public decimal Discount { get; private set; }
        public decimal PriceWithDiscount => Quantity * (Price - Discount);

        public void SetNewDiscount(decimal discount)
        {
            if (discount < 0)
            {
                throw new OrderingDomainException("Discount is not valid");
            }

            Discount = discount;
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity < 0)
            {
                throw new OrderingDomainException("Invalid quantity");
            }

            Quantity += quantity;
        }
    }
}
