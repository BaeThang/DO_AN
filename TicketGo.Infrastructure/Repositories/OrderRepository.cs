using TicketGo.Domain.Entities;
using TicketGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TicketGo.Infrastructure.Data;

namespace TicketGo.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.IdAccountNavigation)
                .Include(o => o.IdDiscountNavigation)
                .Include(o => o.OrderTickets)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.IdAccountNavigation)
                .Include(o => o.IdDiscountNavigation)
                .Include(o => o.OrderTickets)
                .FirstOrDefaultAsync(o => o.IdOrder == id);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found.");
            }

            return order;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(order.IdOrder);
            if (existingOrder == null)
            {
                throw new KeyNotFoundException($"Order with ID {order.IdOrder} not found.");
            }

            _context.Entry(existingOrder).CurrentValues.SetValues(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found.");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetOrdersByAccountIdAsync(int accountId)
        {
            return await _context.Orders
                .Include(o => o.IdAccountNavigation)
                .Include(o => o.IdDiscountNavigation)
                .Include(o => o.OrderTickets)
                    .ThenInclude(ot => ot.IdTicketNavigation)
                    .ThenInclude(t => t.IdSeatNavigation)
                .Include(o => o.OrderTickets)
                    .ThenInclude(ot => ot.IdTicketNavigation)
                    .ThenInclude(t => t.IdTrainNavigation)
                    .ThenInclude(tr => tr.IdTrainRouteNavigation)
                .Where(o => o.IdCus == accountId)
                .ToListAsync();
        }

        // Uncomment and implement if needed
        /*
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.IdOrder == id);
        }
        */
    }
}