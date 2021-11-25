namespace Basket.API.Entities
{
    public class ShoppingCartItem
    {
        public int Quantity { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Discount { get; set; }
    }
}