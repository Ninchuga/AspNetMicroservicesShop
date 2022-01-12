using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.Razor.Models;
using Shopping.Razor.Responses.Order;
using Shopping.Razor.Services;

namespace Shopping.Razor.Pages.Basket
{
    public class CheckoutModel : PageModel
    {
        
        private readonly BasketService _basketService;
        private readonly OrderService _orderService;

        public CheckoutModel(BasketService basketService, OrderService orderService)
        {
            _basketService = basketService;
            _orderService = orderService;
        }

        [BindProperty]
        public BasketCheckout BasketCheckout { get; set; }
        public decimal BasketTotalPrice { get; set; }

        public IActionResult OnGet(decimal basketTotalPrice)
        {
            BasketTotalPrice = basketTotalPrice;
            return Page();
        }

        public async Task<IActionResult> OnPost(BasketCheckout basketCheckout)
        {
            PlaceOrderResponse orderResponse = new PlaceOrderResponse();
            var userNameClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("preferred_userName", StringComparison.OrdinalIgnoreCase));
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            basketCheckout.UserName = userNameClaim.Value;
            basketCheckout.UserId = new Guid(userIdClaim.Value);

            var basketResponse = await _basketService.GetBasketFor(basketCheckout.UserId);
            if(basketResponse.Success)
            {
                orderResponse = await _orderService.PlaceOrder(basketCheckout, basketResponse.BasketWithItems);
            }

            return orderResponse.Success ? RedirectToPage("/Basket/CheckoutComplete") : RedirectToPage("/Error");
        }
    }
}
