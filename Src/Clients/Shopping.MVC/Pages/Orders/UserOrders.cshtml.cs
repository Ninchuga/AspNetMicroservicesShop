using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.MVC.Models;
using Shopping.MVC.Services;

namespace Shopping.MVC.Pages.Orders
{
    public class UserOrdersModel : PageModel
    {
        private readonly OrderService _orderService;

        public UserOrdersModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public List<UserOrder> Orders { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var userIdClaim = User.Claims.FirstOrDefault(claim => claim.Type.Equals("sub", StringComparison.OrdinalIgnoreCase));
            Orders = await _orderService.GetOrdersFor(new Guid(userIdClaim.Value));

            return Page();
        }
    }
}
