using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.Razor.Models;
using Shopping.Razor.Services;

namespace Shopping.Razor.Pages.Basket
{
    public class UserBasketModel : PageModel
    {
        public ShoppingBasket UserBasket { get; set; }

        private readonly BasketService _basketService;

        public UserBasketModel(BasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IActionResult> OnPostDeleteBasketItem(string itemId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            var response = await _basketService.DeleteBasketItem(itemId);
            if (response.Success)
            {
                UserBasket = response.BasketWithItems;
                UserBasket.UserName = string.IsNullOrWhiteSpace(UserBasket.UserName)
                    ? User.Claims.FirstOrDefault(claim => claim.Type.Equals("given_name", StringComparison.OrdinalIgnoreCase))?.Value
                    : UserBasket.UserName;

                return Page();
            }
            else
            {
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnGet()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            var response = await _basketService.GetBasketFor(new Guid(userIdClaim.Value));
            if(response.Success)
            {
                UserBasket = response.BasketWithItems;
                UserBasket.UserName = string.IsNullOrWhiteSpace(UserBasket.UserName)
                    ? User.Claims.FirstOrDefault(claim => claim.Type.Equals("given_name", StringComparison.OrdinalIgnoreCase))?.Value
                    : UserBasket.UserName;

                return Page();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToPage("/Authorization/AccessDenied");
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return RedirectToPage("/Error", new { errorMessage = "Service is currently not available, please try again later." });
            else
                return RedirectToPage("/Error");
        }

        public async Task<IActionResult> OnPostDeleteBasket()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            var response = await _basketService.DeleteBasket(new Guid(userIdClaim.Value));
            return response.Success ? RedirectToPage("/Catalog") : RedirectToPage("/Error");
        }

        public IActionResult OnGetCheckout(decimal basketTotalPrice)
        {
            return RedirectToPage("/Basket/Checkout", new { basketTotalPrice });
        }
    }
}
