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
    public class UserBasketModel : PageModel
    {
        public BasketWithItems UserBasket { get; set; }

        private readonly BasketService _basketService;

        public UserBasketModel(BasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IActionResult> OnPostDeleteBasketItem(string itemId)
        {
            UserBasket = await _basketService.DeleteBasketItem(itemId);

            return Page();
        }

        public async Task<IActionResult> OnGet()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            UserBasket = await _basketService.GetBasketFor(new Guid(userIdClaim.Value));

            var userNameClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("given_name", StringComparison.OrdinalIgnoreCase));
            UserBasket.UserName = userNameClaim.Value;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteBasket()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            await _basketService.DeleteBasket(new Guid(userIdClaim.Value));

            return RedirectToPage("/Catalog");
        }

        public IActionResult OnGetCheckout()
        {
            return RedirectToPage("/Basket/Checkout");
        }
    }
}
