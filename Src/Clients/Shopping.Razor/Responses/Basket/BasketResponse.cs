using Shopping.Razor.Models;

namespace Shopping.Razor.Responses.Basket
{
    public class BasketResponse : BaseResponse
    {
        public ShoppingBasket BasketWithItems { get; set; }
    }
}
