using TicketGo.Domain.Entities;

namespace TicketGo.Application.DTOs
{
    public class OrderTicketDto
    {
        public Train Train { get; set; } = null!;
        public int IdTrain { get; set; }
        public List<Seat> OccupiedSeats { get; set; } = new List<Seat>();
        public string PointStart { get; set; } = string.Empty;
        public string PointEnd { get; set; } = string.Empty;
        public string DateStart { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
    }
}