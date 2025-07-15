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
            // Tạo order trước
            var order = new Order
            {
                UnitPrice = orderDto.TotalPrice,
                DateOrder = orderDto.DateOrder ?? DateTime.Now,
                NameCus = orderDto.NameCus ?? string.Empty,
                Phone = orderDto.Phone ?? string.Empty,
                IdCus = orderDto.IdAccount,
                IdDiscount = orderDto.IdDiscount,
                IdTicket = 0 // Tạm thời set = 0, sẽ update sau
            };

            await _orderRepository.AddAsync(order);

            // Tạo tickets và order tickets
            foreach (var seatName in orderDto.ListSeats)
            {
                if (orderDto.IdCoach == null)
                    throw new ArgumentException("IdCoach is required");

                var seat = await _seatRepository.GetByNameAndCoachIdAsync(seatName, orderDto.IdCoach.Value);

                if (seat != null)
                {
                    // Cập nhật trạng thái ghế
                    seat.State = true;
                    await _seatRepository.UpdateAsync(seat);
                    
                    var coach = await _coachRepository.GetCoachWithRelatedDataAsync(seat.IdCoach);

                    // Tạo ticket
                    var ticket = new Ticket
                    {
                        Date = DateTime.Now,
                        Price = orderDto.TotalPrice ?? 0,
                        IdSeat = seat.IdSeat ?? 0,
                        IdTrain = coach?.IdTrain ?? 0
                    };

                    await _ticketRepository.AddAsync(ticket);

                    // Tạo order ticket
                    var orderTicket = new OrderTicket
                    {
                        IdOrder = order.IdOrder,
                        IdTicket = ticket.IdTicket
                    };

                    await _orderTicketRepository.AddAsync(orderTicket);
                    
                    // Cập nhật IdTicket cho order (lấy ticket đầu tiên)
                    if (order.IdTicket == 0)
                    {
                        order.IdTicket = ticket.IdTicket;
                        await _orderRepository.UpdateAsync(order);
                    }
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

        public async Task<List<OrderDto>> GetOrdersByAccountIdAsync(int accountId)
        {
            var orders = await _orderRepository.GetOrdersByAccountIdAsync(accountId);
            var result = new List<OrderDto>();
            
            foreach (var order in orders)
            {
                var orderDto = new OrderDto
                {
                    IdOrder = order.IdOrder,
                    TotalPrice = order.UnitPrice,
                    DateOrder = order.DateOrder,
                    IdTicket = order.IdTicket,
                    IdDiscount = order.IdDiscount,
                    DiscountName = order.IdDiscountNavigation?.IdDiscount.ToString(),
                    NameCus = order.NameCus ?? string.Empty,
                    Phone = order.Phone ?? string.Empty,
                    IdAccount = order.IdCus,
                    ListSeats = new List<string>(),
                    Status = "Active" // Mặc định là Active
                };

                // Lấy thông tin ghế và tuyến đường từ OrderTicket
                foreach (var orderTicket in order.OrderTickets)
                {
                    var ticket = orderTicket.IdTicketNavigation;
                    if (ticket != null && ticket.IdSeatNavigation != null)
                    {
                        orderDto.ListSeats.Add(ticket.IdSeatNavigation.NameSeat);
                        orderDto.IdCoach = ticket.IdSeatNavigation.IdCoach;
                        
                        // Lấy thông tin tàu và tuyến đường
                        var train = ticket.IdTrainNavigation;
                        if (train != null)
                        {
                            orderDto.TrainName = train.NameTrain;
                            orderDto.DepartureTime = train.DateStart;
                            
                            var trainRoute = train.IdTrainRouteNavigation;
                            if (trainRoute != null)
                            {
                                orderDto.PointStart = trainRoute.PointStart;
                                orderDto.PointEnd = trainRoute.PointEnd;
                            }
                        }
                    }
                }

                result.Add(orderDto);
            }

            return result;
        }
    }
}