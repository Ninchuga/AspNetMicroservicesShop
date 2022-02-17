namespace Shopping.Razor.Models
{
    public class ShoppingBasketItem
    {
        public string ProductId { get; init; }

        public string ProductName { get; init; }

        public decimal Price { get; init; }

        public decimal Discount { get; init; }

        public int Quantity { get; init; }

        public decimal PriceWithDiscount { get; set; }
    }
}
