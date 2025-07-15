using System.ComponentModel.DataAnnotations;

namespace TicketGo.Application.DTOs
{
    public class DashboardStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalTrains { get; set; }
        public int TotalRoutes { get; set; }
        public int TotalTicketsSold { get; set; }
        
        public List<TopRouteDto> TopRoutes { get; set; } = new List<TopRouteDto>();
        public List<TopTrainDto> TopTrains { get; set; } = new List<TopTrainDto>();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new List<MonthlyRevenueDto>();
        public List<DailyRevenueDto> RecentDailyRevenue { get; set; } = new List<DailyRevenueDto>();
        
        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }
        
        public int TodayOrders { get; set; }
        public int WeekOrders { get; set; }
        public int MonthOrders { get; set; }
        public int YearOrders { get; set; }
    }

    public class TopRouteDto
    {
        public string RouteName { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopTrainDto
    {
        public string TrainName { get; set; }
        public string RouteName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
