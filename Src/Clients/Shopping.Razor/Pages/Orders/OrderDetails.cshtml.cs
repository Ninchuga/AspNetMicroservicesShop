using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.Razor.Models;
using Shopping.Razor.Services;

namespace Shopping.Razor.Pages.Orders
{
    public class OrderDetailsModel : PageModel
    {
        private readonly OrderService _orderService;

        public OrderDetailsModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public UserOrder Order { get; set; }

        public async Task<IActionResult> OnGet(Guid orderId)
        {
            Order = await _orderService.GetOrderBy(orderId);

            return Page();
        }
    }
}
