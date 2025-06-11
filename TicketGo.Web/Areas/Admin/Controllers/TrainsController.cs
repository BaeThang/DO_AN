using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketGo.Application.Interfaces;
using TicketGo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace TicketGo.Web.Areas.Admin.Controllers
{   
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class TrainsController : Controller
    {
        private readonly ITrainService _trainService;

        private readonly ITrainRouteService _trainRouteService;

        public TrainsController(ITrainService trainService, ITrainRouteService trainRouteService)
        {
            _trainService = trainService;
            _trainRouteService = trainRouteService;
        }
        // [Danh sách chuyến xe]
        // GET: Admin/Trains
        public async Task<IActionResult> Index()
        {
            var trains = await _trainService.GetAllTrainsAsync();
            var trainroutes = await _trainRouteService.GetAllTrainRoutesAsync(); 
            ViewData["TrainRoutes"] = trainroutes; 
            return View(trains);
        }

        // [Thêm chuyến xe]
        // POST: Admin/Trains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] TrainDto trainDto)
        {
            // Server-side validation
            if (string.IsNullOrWhiteSpace(trainDto.NameTrain))
            {
                ModelState.AddModelError("NameTrain", "Tên chuyến xe không được để trống");
            }
            else if (!Regex.IsMatch(trainDto.NameTrain, @"^[\p{L}\p{N}\s]+$"))
            {
                ModelState.AddModelError("NameTrain", "Tên chuyến xe chỉ được chứa chữ cái, số và khoảng trắng");
            }
            if (trainDto.DateStart < DateTime.Now)
            {
                ModelState.AddModelError("DateStart", "Ngày khởi hành phải là ngày trong tương lai");
            }
            if (trainDto.CoefficientTrain <= 0)
            {
                ModelState.AddModelError("CoefficientTrain", "Hệ số chuyến xe phải lớn hơn 0");
            }

            if (!ModelState.IsValid)
            {
                var trainRoutes = await _trainService.GetAllTrainRoutesAsync();
                ViewData["IdTrainRoute"] = new SelectList(trainRoutes, "IdTrainRoute", "PointStart", trainDto.IdTrainRoute);

                TempData["ErrorMessages"] = "Error: Dữ liệu không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _trainService.CreateTrainAsync(trainDto);
                TempData["SuccessMessage"] = "Chuyến xe đã được thêm!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo chuyến xe: " + ex.Message);
                var trainRoutes = await _trainService.GetAllTrainRoutesAsync();
                ViewData["IdTrainRoute"] = new SelectList(trainRoutes, "IdTrainRoute", "PointStart", trainDto.IdTrainRoute);
                return RedirectToAction(nameof(Index));
            }
        }

        // [Sửa chuyến xe]
        // POST: Admin/Trains/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] TrainDto trainDto)
        {
            // Server-side validation
            if (string.IsNullOrWhiteSpace(trainDto.NameTrain))
            {
                ModelState.AddModelError("NameTrain", "Tên chuyến xe không được để trống");
            }
            else if (!trainDto.NameTrain.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
            {
                ModelState.AddModelError("NameTrain", "Tên chuyến xe chỉ được chứa chữ cái, số và khoảng trắng");
            }
            if (trainDto.DateStart < DateTime.Now)
            {
                ModelState.AddModelError("DateStart", "Ngày khởi hành phải là ngày trong tương lai");
            }
            if (trainDto.CoefficientTrain <= 0)
            {
                ModelState.AddModelError("CoefficientTrain", "Hệ số chuyến xe phải lớn hơn 0");
            }

            if (!ModelState.IsValid)
            {
                var trainRoutes = await _trainService.GetAllTrainRoutesAsync();
                ViewData["IdTrainRoute"] = new SelectList(trainRoutes, "IdTrainRoute", "PointStart", trainDto.IdTrainRoute);
                
                TempData["ErrorMessages"] = "Error: Dữ liệu không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existingTrain = await _trainService.GetTrainByIdAsync(trainDto.IdTrain.Value);
                if (existingTrain == null) return NotFound();

                await _trainService.UpdateTrainAsync(trainDto.IdTrain.Value, trainDto);
                TempData["SuccessMessage"] = "Chuyến xe đã được cập nhật!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật chuyến xe: " + ex.Message);
                var trainRoutes = await _trainService.GetAllTrainRoutesAsync();
                ViewData["IdTrainRoute"] = new SelectList(trainRoutes, "IdTrainRoute", "PointStart", trainDto.IdTrainRoute);
                return RedirectToAction(nameof(Index));
            }
        }

        // [Xóa chuyến xe]
        // POST: Admin/Trains/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var train = await _trainService.GetTrainByIdAsync(id);
            if (train == null)
            {
                return NotFound();
            }

            try
            {
                await _trainService.DeleteTrainAsync(id);
                TempData["SuccessMessage"] = "Đã xóa chuyến xe!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa chuyến xe: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}