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
    public class AddToBasketModel : PageModel
    {
        private readonly BasketService _basketService;
        private readonly CatalogService _catalogService;


        public AddToBasketModel(BasketService basketService, CatalogService catalogService)
        {
            _basketService = basketService;
            _catalogService = catalogService;
        }

        public async Task<IActionResult> OnGet(string itemId)
        {
            var catalogItem = await _catalogService.GetCatalogItemBy(itemId);
            await _basketService.AddItemToBasket(catalogItem);

            return RedirectToPage("/Catalog");
        }

    }
}
