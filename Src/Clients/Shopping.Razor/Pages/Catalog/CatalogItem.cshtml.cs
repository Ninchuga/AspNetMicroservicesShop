using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.Razor.Models;
using Shopping.Razor.Services;

namespace Shopping.Razor.Pages.Catalog
{
    public class CatalogItemModel : PageModel
    {
        public CatalogItem CatalogItem { get; set; }

        private readonly CatalogService _catalogService;

        public CatalogItemModel(CatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public async Task<IActionResult> OnGet(string itemId)
        {
            var response = await _catalogService.GetCatalogItemBy(itemId);
            if (response.Success)
            {
                CatalogItem = response.CatalogItem;
                return Page();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToPage("/Authorization/AccessDenied");
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return RedirectToPage("/Error", new { errorMessage = "Service is currently not available, please try again later." });
            else
                return RedirectToPage("/Error");
        }
    }
}
