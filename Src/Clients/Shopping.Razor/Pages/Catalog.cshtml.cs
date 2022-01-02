using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.Razor.Models;
using Shopping.Razor.Services;

namespace Shopping.Razor.Pages
{
    public class CatalogModel : PageModel
    {
        private readonly CatalogService _catalogService;
        private readonly BasketService _basketService;

        public IReadOnlyList<CatalogItem> CatalogItems { get; set; } = new List<CatalogItem>();

        public CatalogModel(CatalogService catalogService, BasketService basketService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
        }

        public async Task<IActionResult> OnPostAddItemToBasket(int itemQuantity, string itemId)
        {
            var response = await _catalogService.GetCatalogItemBy(itemId);
            if(response.Success)
            {
                response.CatalogItem.Quantity = itemQuantity;
                await _basketService.AddItemToBasket(response.CatalogItem);

                return RedirectToPage("/Basket/UserBasket");
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return RedirectToPage("/Error", new { errorMessage = "Service is currently not available, please try again later." });
            else
                return RedirectToPage("/Error");
        }

        public async Task<IActionResult> OnGet()
        {
            await WriteOutIdentityInformation();

            var response = await _catalogService.GetCatalog();
            if (response.Success)
            {
                CatalogItems = response.CatalogItems;
                return Page();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToPage("/Authorization/AccessDenied");
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return RedirectToPage("/Error", new { errorMessage = "Service is currently not available, please try again later." });
            else 
                return RedirectToPage("/Error");
        }

        // TODO: check if this method is needed
        public IActionResult OnGetCatalogItem(string itemId)
        {
            //var response = await _catalogService.GetCatalogItemBy(itemId);

            // redirect to a page with single catalog item
            return RedirectToPage("/Catalog/CatalogItem", new { itemId });
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
