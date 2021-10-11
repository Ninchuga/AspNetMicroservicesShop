using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shopping.MVC.Models;
using Shopping.MVC.Services;

namespace Shopping.MVC.Pages
{
    public class CatalogModel : PageModel
    {
        private readonly CatalogService _catalogService;
        private readonly BasketService _basketService;

        public IEnumerable<CatalogItem> CatalogItems { get; set; } = new List<CatalogItem>();

        public CatalogModel(CatalogService catalogService, BasketService basketService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
        }

        public async Task<IActionResult> OnPostAddItemToBasket(int itemQuantity, string itemId)
        {
            var catalogItem = await _catalogService.GetCatalogItemBy(itemId);
            catalogItem.Quantity = itemQuantity;
            await _basketService.AddItemToBasket(catalogItem);

            return RedirectToPage("/Basket/UserBasket");
        }

        public async Task<IActionResult> OnGet()
        {
            await WriteOutIdentityInformation();

            CatalogItems = await _catalogService.GetCatalog();

            return CatalogItems == null ? RedirectToPage("/Authorization/AccessDenied") : Page();
        }

        public async Task<IActionResult> OnGetCatalogItem(string itemId)
        {
            var catalogItem = await _catalogService.GetCatalogItemBy(itemId);

            return RedirectToPage("/");
        }

        public async Task WriteOutIdentityInformation()
        {
            // get the saved indetity token
           // var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            var identityToken = await HttpContext.GetTokenAsync("id_token");

            // write it out
            Debug.WriteLine($"Identity token: {identityToken}");

            // write out the user claims
            foreach (var claim in User.Claims)
            {
                Debug.WriteLine($"Claim type: {claim.Type} - Claim value: {claim.Value}");
            }
        }
    }
}
