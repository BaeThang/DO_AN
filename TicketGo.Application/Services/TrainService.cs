using TicketGo.Application.DTOs;
using TicketGo.Domain.Entities;
using TicketGo.Domain.Interfaces;
using TicketGo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TicketGo.Application.Services
{
    public class TrainService : ITrainService
    {
        private readonly ITrainRouteRepository _trainRouteRepository;
        private readonly ITrainRepository _trainRepository;

        public TrainService(ITrainRouteRepository trainRouteRepository, ITrainRepository trainRepository)
        {
            _trainRouteRepository = trainRouteRepository;
            _trainRepository = trainRepository;
        }

        public async Task<List<string>> GetStartPointsAsync(string term)
        {
            return await _trainRouteRepository.GetStartPointsAsync(term);
        }

        public async Task<List<string>> GetEndPointsAsync(string term)
        {
            return await _trainRouteRepository.GetEndPointsAsync(term);
        }

        public async Task<PagedResult<TrainResponseDto>> SearchTrainsAsync(TrainSearchRequest request)
        {
            // Gọi repository với các tham số riêng lẻ
            var pagedTrains = await _trainRepository.SearchTrainsAsync(
                noiDi: request.NoiDi,
                noiDen: request.NoiDen,
                ngayKhoiHanh: request.NgayKhoiHanh,
                sortTime: request.SortTime,
                sortPrice: request.SortPrice,
                loaiXe: request.LoaiXe ?? new List<string>(),
                page: request.Page,
                pageSize: request.PageSize);

            // Ánh xạ Train sang TrainResponseDto, xử lý null
            var trainDtos = pagedTrains.Items.Select(train =>
            {
                var coach = train.Coaches.FirstOrDefault(c => request.LoaiXe.Count == 0 || request.LoaiXe.Contains(c.Category));
                var basicPrice = (decimal)(coach?.BasicPrice ?? 0);
                var coefficientTrain = (decimal)(train.CoefficientTrain ?? 1);
                var finalPrice = coefficientTrain * basicPrice;
                
                return new TrainResponseDto
                {
                    Id = train.IdTrain,
                    TenTau = train.NameTrain ?? "N/A",
                    NoiDi = train.IdTrainRouteNavigation?.PointStart ?? "N/A",
                    NoiDen = train.IdTrainRouteNavigation?.PointEnd ?? "N/A",
                    GioKhoiHanh = train.DateStart.HasValue ? train.DateStart.Value : default(DateTime),
                    GiaVe = coach != null ? finalPrice : null,
                    LoaiXe = coach?.Category ?? "N/A",
                    CoachID = coach?.IdCoach ?? 0
                };
            }).ToList();

            // Trả về kết quả phân trang
            return new PagedResult<TrainResponseDto>
            {
                Items = trainDtos,
                Page = pagedTrains.Page,
                PageSize = pagedTrains.PageSize,
                TotalRecords = pagedTrains.TotalRecords
            };
        }

        public async Task<List<TrainDto>> GetAllTrainsAsync()
        {
            var trains = await _trainRepository.GetAllAsync();
            return trains.Select(t => new TrainDto
            {
                IdTrain = t.IdTrain,
                NameTrain = t.NameTrain,
                DateStart = t.DateStart,
                IdTrainRoute = t.IdTrainRoute,
                TrainRouteName = t.IdTrainRouteNavigation != null
                    ? $"{t.IdTrainRouteNavigation.PointStart} - {t.IdTrainRouteNavigation.PointEnd}"
                    : "N/A",
                CoefficientTrain = (double?)t.CoefficientTrain
            }).ToList();
        }

        public async Task<TrainDto> GetTrainByIdAsync(int id)
        {
            var train = await _trainRepository.GetByIdAsync(id);
            if (train == null)
            {
                return null;
            }

            return new TrainDto
            {
                IdTrain = train.IdTrain,
                NameTrain = train.NameTrain,
                DateStart = train.DateStart,
                IdTrainRoute = train.IdTrainRoute,
                TrainRouteName = train.IdTrainRouteNavigation != null
                    ? $"{train.IdTrainRouteNavigation.PointStart} - {train.IdTrainRouteNavigation.PointEnd}"
                    : "N/A",
                CoefficientTrain = (double?)train.CoefficientTrain
            };
        }

        public async Task CreateTrainAsync(TrainDto trainDto)
        {
            var train = new Train
            {
                NameTrain = trainDto.NameTrain,
                DateStart = trainDto.DateStart,
                IdTrainRoute = trainDto.IdTrainRoute,
                CoefficientTrain = (decimal?)trainDto.CoefficientTrain
            };

            await _trainRepository.AddAsync(train);
        }

        public async Task UpdateTrainAsync(int id, TrainDto trainDto)
        {
            var train = await _trainRepository.GetByIdAsync(id);
            if (train == null)
            {
                throw new Exception("Train not found");
            }

            train.NameTrain = trainDto.NameTrain;
            train.DateStart = trainDto.DateStart;
            train.IdTrainRoute = trainDto.IdTrainRoute;
            train.CoefficientTrain = (decimal?)trainDto.CoefficientTrain;

            await _trainRepository.UpdateAsync(train);
        }

        public async Task DeleteTrainAsync(int id)
        {
            await _trainRepository.DeleteAsync(id);
        }

        public async Task<List<TrainRouteDto>> GetAllTrainRoutesAsync()
        {
            var trainRoutes = await _trainRouteRepository.GetAllAsync();
            return trainRoutes.Select(tr => new TrainRouteDto
            {
                IdTrainRoute = tr.IdTrainRoute,
                PointStart = tr.PointStart,
                PointEnd = tr.PointEnd
            }).ToList();
        }
    }
}