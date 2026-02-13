using CourseWork.Constants;
using CourseWork.Core.Models;
using CourseWork.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Controllers
{
    [Authorize(Roles = UserRoles.AdminOrManager)]

    public class BrandsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public BrandsController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Home/FormBrand.cshtml", new Brand());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/FormBrand.cshtml", brand);

            try
            {
                await _unitOfWork.Brands.AddAsync(brand);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = $"Бренд '{brand.BrandName}' создан.";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Ошибка збережена. Може, бренд вже існує.");
                return View("~/Views/Home/FormBrand.cshtml", brand);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null) return NotFound();
            return View("~/Views/Home/FormBrand.cshtml", brand);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Brand brand)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/FormBrand.cshtml", brand);

            try
            {
                var existing = await _unitOfWork.Brands.GetByIdAsync(brand.BrandId);
                if (existing == null) return NotFound();

                existing.BrandName = brand.BrandName;
                existing.Country = brand.Country;

                _unitOfWork.Brands.Update(existing);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Бренд оновлено.";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("BrandName", "Бренд з такою назвою вже існує.");
                return View("~/Views/Home/FormBrand.cshtml", brand);
            }
        }
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null) return NotFound();

            try
            {
                _unitOfWork.Brands.Remove(brand);
                await _unitOfWork.SaveAsync();
                TempData["Success"] = "Бренд удален.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неможливо видалити  бренд, так як у нього вже є продукти.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}