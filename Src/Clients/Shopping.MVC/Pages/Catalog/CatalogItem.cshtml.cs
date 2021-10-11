using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.MVC.Models;
using Shopping.MVC.Services;

namespace Shopping.MVC.Pages.Catalog
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
            CatalogItem = await _catalogService.GetCatalogItemBy(itemId);

            return Page();
        }
    }
}
