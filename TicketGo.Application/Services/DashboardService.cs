using TicketGo.Application.DTOs;
using TicketGo.Application.Interfaces;
using TicketGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TicketGo.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITrainRepository _trainRepository;
        private readonly ITrainRouteRepository _trainRouteRepository;
        private readonly IAccountRepository _accountRepository;

        public DashboardService(
            IOrderRepository orderRepository,
            ITrainRepository trainRepository,
            ITrainRouteRepository trainRouteRepository,
            IAccountRepository accountRepository)
        {
            _orderRepository = orderRepository;
            _trainRepository = trainRepository;
            _trainRouteRepository = trainRouteRepository;
            _accountRepository = accountRepository;
        }

        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var trains = await _trainRepository.GetAllAsync();
            var routes = await _trainRouteRepository.GetAllAsync();
            var accounts = await _accountRepository.GetAllAsync();

            var now = DateTime.Now;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            var ordersWithPrices = orders.Where(o => o.UnitPrice.HasValue && o.DateOrder.HasValue);

            var statistics = new DashboardStatisticsDto
            {
                TotalOrders = orders.Count,
                TotalRevenue = (decimal)(ordersWithPrices.Sum(o => o.UnitPrice ?? 0)),
                TotalCustomers = accounts.Count(a => a.IdRole == 2), // Assuming role 2 is customer
                TotalTrains = trains.Count,
                TotalRoutes = routes.Count,
                TotalTicketsSold = orders.Count, // Using orders count as tickets sold

                // Today statistics
                TodayRevenue = (decimal)(ordersWithPrices
                    .Where(o => o.DateOrder!.Value.Date == today)
                    .Sum(o => o.UnitPrice ?? 0)),
                TodayOrders = orders.Count(o => o.DateOrder.HasValue && o.DateOrder.Value.Date == today),

                // Week statistics
                WeekRevenue = (decimal)(ordersWithPrices
                    .Where(o => o.DateOrder!.Value.Date >= weekStart && o.DateOrder.Value.Date <= today)
                    .Sum(o => o.UnitPrice ?? 0)),
                WeekOrders = orders.Count(o => o.DateOrder.HasValue && 
                    o.DateOrder.Value.Date >= weekStart && o.DateOrder.Value.Date <= today),

                // Month statistics
                MonthRevenue = (decimal)(ordersWithPrices
                    .Where(o => o.DateOrder!.Value.Date >= monthStart && o.DateOrder.Value.Date <= today)
                    .Sum(o => o.UnitPrice ?? 0)),
                MonthOrders = orders.Count(o => o.DateOrder.HasValue && 
                    o.DateOrder.Value.Date >= monthStart && o.DateOrder.Value.Date <= today),

                // Year statistics
                YearRevenue = (decimal)(ordersWithPrices
                    .Where(o => o.DateOrder!.Value.Date >= yearStart && o.DateOrder.Value.Date <= today)
                    .Sum(o => o.UnitPrice ?? 0)),
                YearOrders = orders.Count(o => o.DateOrder.HasValue && 
                    o.DateOrder.Value.Date >= yearStart && o.DateOrder.Value.Date <= today),

                TopRoutes = await GetTopRoutesAsync(5),
                TopTrains = await GetTopTrainsAsync(5),
                MonthlyRevenue = await GetMonthlyRevenueAsync(now.Year),
                RecentDailyRevenue = await GetDailyRevenueAsync(today.AddDays(-30), today)
            };

            return statistics;
        }

        public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year)
        {
            var orders = await _orderRepository.GetAllAsync();
            
            var monthlyRevenue = orders
                .Where(o => o.DateOrder.HasValue && o.UnitPrice.HasValue && o.DateOrder.Value.Year == year)
                .GroupBy(o => new { o.DateOrder!.Value.Month, o.DateOrder.Value.Year })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = (decimal)g.Sum(o => o.UnitPrice ?? 0),
                    Orders = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            // Fill missing months with 0
            var result = new List<MonthlyRevenueDto>();
            for (int month = 1; month <= 12; month++)
            {
                var existing = monthlyRevenue.FirstOrDefault(m => m.Month == month);
                result.Add(existing ?? new MonthlyRevenueDto
                {
                    Month = month,
                    Year = year,
                    Revenue = 0,
                    Orders = 0
                });
            }

            return result;
        }

        public async Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            var orders = await _orderRepository.GetAllAsync();
            
            return orders
                .Where(o => o.DateOrder.HasValue && o.UnitPrice.HasValue && 
                           o.DateOrder.Value.Date >= fromDate && o.DateOrder.Value.Date <= toDate)
                .GroupBy(o => o.DateOrder!.Value.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = (decimal)g.Sum(o => o.UnitPrice ?? 0),
                    Orders = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        public async Task<List<TopRouteDto>> GetTopRoutesAsync(int top = 5)
        {
            var orders = await _orderRepository.GetAllAsync();
            var trains = await _trainRepository.GetAllAsync();
            var routes = await _trainRouteRepository.GetAllAsync();

            var topRoutes = from order in orders
                           join train in trains on order.IdTicket equals train.IdTrain
                           join route in routes on train.IdTrainRoute equals route.IdTrainRoute
                           where order.UnitPrice.HasValue
                           group order by new { route.IdTrainRoute, route.PointStart, route.PointEnd } into g
                           orderby g.Sum(o => o.UnitPrice ?? 0) descending
                           select new TopRouteDto
                           {
                               RouteName = $"{g.Key.PointStart} - {g.Key.PointEnd}",
                               StartPoint = g.Key.PointStart,
                               EndPoint = g.Key.PointEnd,
                               TotalOrders = g.Count(),
                               TotalRevenue = (decimal)g.Sum(o => o.UnitPrice ?? 0)
                           };

            return topRoutes.Take(top).ToList();
        }

        public async Task<List<TopTrainDto>> GetTopTrainsAsync(int top = 5)
        {
            var orders = await _orderRepository.GetAllAsync();
            var trains = await _trainRepository.GetAllAsync();
            var routes = await _trainRouteRepository.GetAllAsync();

            var topTrains = from order in orders
                           join train in trains on order.IdTicket equals train.IdTrain
                           join route in routes on train.IdTrainRoute equals route.IdTrainRoute
                           where order.UnitPrice.HasValue
                           group order by new { train.IdTrain, train.NameTrain, route.PointStart, route.PointEnd } into g
                           orderby g.Sum(o => o.UnitPrice ?? 0) descending
                           select new TopTrainDto
                           {
                               TrainName = g.Key.NameTrain,
                               RouteName = $"{g.Key.PointStart} - {g.Key.PointEnd}",
                               TotalOrders = g.Count(),
                               TotalRevenue = (decimal)g.Sum(o => o.UnitPrice ?? 0)
                           };

            return topTrains.Take(top).ToList();
        }
    }
}
