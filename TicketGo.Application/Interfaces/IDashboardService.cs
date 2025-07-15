using TicketGo.Application.DTOs;

namespace TicketGo.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year);
        Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<List<TopRouteDto>> GetTopRoutesAsync(int top = 5);
        Task<List<TopTrainDto>> GetTopTrainsAsync(int top = 5);
    }
}
