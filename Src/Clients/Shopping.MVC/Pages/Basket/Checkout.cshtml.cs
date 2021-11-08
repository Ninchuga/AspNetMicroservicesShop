using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.MVC.Models;
using Shopping.MVC.Services;

namespace Shopping.MVC.Pages.Basket
{
    public class CheckoutModel : PageModel
    {
        
        private readonly BasketService _basketService;

        public CheckoutModel(BasketService basketService)
        {
            _basketService = basketService;
        }

        [BindProperty]
        public BasketCheckout BasketCheckout { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(BasketCheckout basketCheckout)
        {
            var userNameClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("preferred_userName", StringComparison.OrdinalIgnoreCase));
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            basketCheckout.UserName = userNameClaim.Value;
            basketCheckout.UserId = new Guid(userIdClaim.Value);

            var response = await _basketService.Checkout(basketCheckout);

            return response.Success ? RedirectToPage("/Basket/CheckoutComplete") : RedirectToPage("/Error");
        }
    }
}
