using TicketGo.Application.DTOs;
using TicketGo.Domain.Entities;
using TicketGo.Domain.Interfaces;
using TicketGo.Application.Interfaces;

namespace TicketGo.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IOrderTicketRepository _orderTicketRepository;

        public OrderService(
            ICoachRepository coachRepository,
            IOrderRepository orderRepository,
            ISeatRepository seatRepository,
            ITicketRepository ticketRepository,
            IOrderTicketRepository orderTicketRepository,
            IDiscountRepository discountRepository)
        {
            _coachRepository = coachRepository;
            _orderRepository = orderRepository;
            _seatRepository = seatRepository;
            _ticketRepository = ticketRepository;
            _orderTicketRepository = orderTicketRepository;
            _discountRepository = discountRepository;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => new OrderDto
            {
                IdOrder = o.IdOrder,
                TotalPrice = o.UnitPrice, // Đồng bộ với tên TotalPrice
                DateOrder = o.DateOrder,
                IdTicket = o.IdTicket,
                IdDiscount = o.IdDiscount,
                DiscountName = o.IdDiscountNavigation?.IdDiscount.ToString(),
                NameCus = o.NameCus,
                Phone = o.Phone,
                IdAccount = o.IdAccountNavigation.IdAccount,
            }).ToList();
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return null;
            }

            return new OrderDto
            {
                IdOrder = order.IdOrder,
                TotalPrice = order.UnitPrice,
                DateOrder = order.DateOrder,
                IdTicket = order.IdTicket,
                IdDiscount = order.IdDiscount,
                DiscountName = order.IdDiscountNavigation?.IdDiscount.ToString(),
                NameCus = order.NameCus,
                Phone = order.Phone,
                IdAccount = order.IdAccountNavigation.IdAccount,
            };
        }

        public async Task<OrderTicketDto> GetOrderTicketDetailsAsync(int idCoach)
        {
            var coach = await _coachRepository.GetCoachWithRelatedDataAsync(idCoach);
            if (coach == null || coach.IdTrainNavigation == null)
            {
                return null!;
            }

            var occupiedSeats = coach.Seats.Where(s => s.State).ToList();
            var coachCategory = coach.Category ?? string.Empty;
            var ticketPrice = CalculateTicketPrice(coach.IdTrainNavigation);

            return CreateOrderTicketDto(coach, occupiedSeats, coachCategory, ticketPrice);
        }

        public async Task CreateOrderAsync(OrderDto orderDto)
        {
            var order = new Order
            {
                UnitPrice = orderDto.TotalPrice,
                DateOrder = orderDto.DateOrder ?? DateTime.Now,
                NameCus = orderDto.NameCus,
                Phone = orderDto.Phone,
                IdCus = orderDto.IdAccount,
                IdDiscount = orderDto.IdDiscount ?? null
            };

            await _orderRepository.AddAsync(order);

            foreach (var seatName in orderDto.ListSeats)
            {
                if (orderDto.IdCoach == null)
                    throw new ArgumentException("IdCoach is required");

                var seat = await _seatRepository.GetByNameAndCoachIdAsync(seatName, orderDto.IdCoach.Value);

    
                if (seat != null)
                {
                    seat.State = true;
                    await _seatRepository.UpdateAsync(seat);
                    var coach = await _coachRepository.GetCoachWithRelatedDataAsync(seat.IdCoach);

                    var ticket = new Ticket
                    {
                        Date = DateTime.Now,
                        Price = orderDto.TotalPrice ?? 0,
                        IdSeat = seat.IdSeat.Value,
                        IdTrain = coach.IdTrain.Value
                    };

                    await _ticketRepository.AddAsync(ticket);

                    var orderTicket = new OrderTicket
                    {
                        IdOrder = order.IdOrder,
                        IdTicket = ticket.IdTicket
                    };

                    await _orderTicketRepository.AddAsync(orderTicket);
                }
            }
        }

        private decimal CalculateTicketPrice(Train train)
        {
            var basicPrice = (decimal)(train.Coaches.FirstOrDefault()?.BasicPrice ?? 0);
            return (decimal)(train.CoefficientTrain ?? 1) * basicPrice;
        }

        private OrderTicketDto CreateOrderTicketDto(Coach coach, List<Seat> occupiedSeats, string coachCategory, decimal ticketPrice)
        {
            var train = coach.IdTrainNavigation;
            var totalSeats = coach.SeatsQuantity ?? 0;
            
            return new OrderTicketDto
            {
                Train = train ?? new Train(),
                IdTrain = train?.IdTrain ?? 0,
                OccupiedSeats = occupiedSeats,
                PointStart = train?.IdTrainRouteNavigation?.PointStart ?? string.Empty,
                PointEnd = train?.IdTrainRouteNavigation?.PointEnd ?? string.Empty,
                DateStart = train?.DateStart?.ToShortDateString() ?? string.Empty,
                Price = ticketPrice,
                VehicleType = coachCategory,
                TotalSeats = totalSeats
            };
        }

        public async Task UpdateOrderAsync(int id, OrderDto orderDto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new Exception("Đơn hàng không tồn tại");
            }

            order.UnitPrice = orderDto.TotalPrice ?? 0;
            order.DateOrder = orderDto.DateOrder ?? DateTime.Now;
            order.IdTicket = orderDto.IdTicket ?? 0;
            order.IdDiscount = orderDto.IdDiscount ?? 0;
            order.NameCus = orderDto.NameCus;
            order.Phone = orderDto.Phone;
            order.IdCus = orderDto.IdAccount; // Sử dụng orderDto, không phải Coach

            await _orderRepository.UpdateAsync(order);
        }

        public async Task DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
        }

    }
}