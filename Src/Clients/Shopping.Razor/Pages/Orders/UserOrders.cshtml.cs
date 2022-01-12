using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.Razor.Models;
using Shopping.Razor.Services;

namespace Shopping.Razor.Pages.Orders
{
    public class UserOrdersModel : PageModel
    {
        private readonly OrderService _orderService;

        public UserOrdersModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public IReadOnlyList<UserOrder> Orders { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            var response = await _orderService.GetOrdersFor(new Guid(userIdClaim.Value));
            if(response.Success)
            {
                Orders = response.UserOrder.OrderByDescending(order => order.OrderDate).ToList().AsReadOnly();
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
