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
    public class GetBasketModel : PageModel
    {
        public BasketWithItems UserBasket { get; set; }

        private readonly BasketService _basketService;

        public GetBasketModel(BasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IActionResult> OnGet()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            UserBasket = await _basketService.GetBasketFor(new Guid(userIdClaim.Value));

            var userNameClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("given_name", StringComparison.OrdinalIgnoreCase));
            UserBasket.UserName = userNameClaim.Value;

            return Page();
        }
    }
}
