using TicketGo.Domain.Entities;

namespace TicketGo.Domain.Interfaces
{
    public interface IOrderRepository 
    {
        Task<List<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetOrdersByAccountIdAsync(int accountId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        // Task<bool> ExistsAsync(int id);
    }
}