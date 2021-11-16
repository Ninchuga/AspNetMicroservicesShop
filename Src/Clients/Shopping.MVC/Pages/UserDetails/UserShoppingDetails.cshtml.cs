using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.MVC.Models.Aggregator;
using Shopping.MVC.Services;

namespace Shopping.MVC.Pages.UserDetails
{
    public class UserShoppingDetailsModel : PageModel
    {
        private readonly ShoppingService _shoppingService;

        public UserShoppingDetailsModel(ShoppingService shoppingService)
        {
            _shoppingService = shoppingService;
        }

        public UserShoppingDetails UserShoppingDetails { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            UserShoppingDetails = await _shoppingService.GetUserShoppingDetails(new Guid(userIdClaim.Value));
            if (UserShoppingDetails.Success)
            {
                UserShoppingDetails.BasketWithProducts.UserName = string.IsNullOrWhiteSpace(UserShoppingDetails.BasketWithProducts.UserName)
                    ? User.Claims.FirstOrDefault(claim => claim.Type.Equals("given_name", StringComparison.OrdinalIgnoreCase))?.Value
                    : UserShoppingDetails.BasketWithProducts.UserName;

                return Page();
            }
            else if (UserShoppingDetails.StatusCode == HttpStatusCode.Unauthorized || UserShoppingDetails.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToPage("/Authorization/AccessDenied");
            else if (UserShoppingDetails.StatusCode == HttpStatusCode.ServiceUnavailable)
                return RedirectToPage("/Error", new { errorMessage = "Service is currently not available, please try again later." });
            else
                return RedirectToPage("/Error");
        }
    }
}
