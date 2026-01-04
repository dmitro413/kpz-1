using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class WeightsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public WeightsController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Home/FormWeight.cshtml", new Weight());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Weight weight)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.Weights.AddAsync(weight);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = $"Вагу '{weight.WeightValue} {weight.Unit}' додано.";
                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Така вага вже існує.");
                }
            }
            return View("~/Views/Home/FormWeight.cshtml", weight);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var weight = await _unitOfWork.Weights.GetByIdAsync(id);
            if (weight == null) return NotFound();
            return View("~/Views/Home/FormWeight.cshtml", weight);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Weight weight)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _unitOfWork.Weights.GetByIdAsync(weight.WeightId);
                    if (existing != null)
                    {
                        existing.WeightValue = weight.WeightValue;
                        existing.Unit = weight.Unit;

                        _unitOfWork.Weights.Update(existing);
                        await _unitOfWork.SaveAsync();

                        TempData["Success"] = "Вагу оновлено.";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Така комбінація ваги та одиниці виміру вже існує.");
                }
            }
            return View("~/Views/Home/FormWeight.cshtml", weight);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var weight = await _unitOfWork.Weights.GetByIdAsync(id);
            if (weight != null)
            {
                try
                {
                    _unitOfWork.Weights.Remove(weight);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Вагу видалено.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Неможливо видалити: ця вага використовується в товарах.";
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}