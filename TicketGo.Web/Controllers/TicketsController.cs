using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketGo.Application.Interfaces;
using System.Security.Claims;

namespace TicketGo.Web.Controllers
{
    public class TicketsController : Controller
    {
        private readonly IOrderService _orderService;

        public TicketsController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> ManagerTicket()
        {
            // Lấy userId từ session hoặc claims
            var userId = HttpContext.Session.GetInt32("AccountID");
            if (userId == null)
            {
                // Thử lấy từ claims
                var userIdClaim = User.FindFirst("UserID");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int claimUserId))
                {
                    userId = claimUserId;
                }
                else
                {
                    return RedirectToAction("Login", "Access");
                }
            }

            var userOrders = await _orderService.GetOrdersByAccountIdAsync(userId.Value);
            
            return View(userOrders);
        }

        // Action để test dữ liệu
        public async Task<IActionResult> TestData()
        {
            var allOrders = await _orderService.GetAllOrdersAsync();
            return Json(new { 
                TotalOrders = allOrders.Count, 
                Orders = allOrders.Select(o => new { 
                    o.IdOrder, 
                    o.NameCus, 
                    o.Phone, 
                    o.IdAccount,
                    o.TotalPrice,
                    o.DateOrder
                })
            });
        }
    }
}
