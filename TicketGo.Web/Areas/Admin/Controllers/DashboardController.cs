using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketGo.Application.Interfaces;
using TicketGo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace TicketGo.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var statistics = await _dashboardService.GetDashboardStatisticsAsync();
            return View(statistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyRevenue(int year)
        {
            var monthlyData = await _dashboardService.GetMonthlyRevenueAsync(year);
            return Json(monthlyData);
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyRevenue(DateTime fromDate, DateTime toDate)
        {
            var dailyData = await _dashboardService.GetDailyRevenueAsync(fromDate, toDate);
            return Json(dailyData);
        }
    }
}