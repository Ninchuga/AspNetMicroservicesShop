namespace Basket.API.Entities
{
    public class ShoppingBasketItem
    {
        public ShoppingBasketItem(int quantity, string color, decimal price, string productId, string productName, decimal discount)
        {
            Quantity = quantity;
            Color = color;
            Price = price;
            ProductId = productId;
            ProductName = productName;
            Discount = discount;
        }

        public int Quantity { get; private set; }
        public string Color { get; private set; }
        public decimal Price { get; private set; }
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal Discount { get; private set; }
        public decimal PriceWithDiscount => Price - Discount;

        public ShoppingBasketItem UpdateQuantity(int quantity)
        {
            Quantity += quantity;

            return this;
        }

        public void UpdateDiscount(decimal discount)
        {
            Discount = discount;
        }

        public void UpdatePrice()
        {
            Price *= Quantity;
        }
    }
}