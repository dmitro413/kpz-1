using CourseWork.Models;
using CourseWork.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class VariantsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public VariantsController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View("~/Views/Home/FormVariant.cshtml", new ProductVariant());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVariant variant)
        {
            ModelState.Remove("Product");
            ModelState.Remove("Weight");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(variant.ProductId, variant.WeightId);
                return View("~/Views/Home/FormVariant.cshtml", variant);
            }

            try
            {
                variant.CreatedAt = DateTime.UtcNow;
                variant.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductVariants.AddAsync(variant);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Варіант успішно створено";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Помилка при збереженні. Перевірте дані.");
                await PopulateDropdowns(variant.ProductId, variant.WeightId);
                return View("~/Views/Home/FormVariant.cshtml", variant);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(id);
            if (variant == null)
            {
                TempData["Error"] = "Варіант не знайдено";
                return RedirectToAction("Index", "Home");
            }

            await PopulateDropdowns(variant.ProductId, variant.WeightId);
            return View("~/Views/Home/FormVariant.cshtml", variant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductVariant variant)
        {
            ModelState.Remove("Product");
            ModelState.Remove("Weight");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(variant.ProductId, variant.WeightId);
                return View("~/Views/Home/FormVariant.cshtml", variant);
            }

            try
            {
                var existing = await _unitOfWork.ProductVariants.GetByIdAsync(variant.VariantId);
                if (existing == null) return NotFound();

                existing.ProductId = variant.ProductId;
                existing.WeightId = variant.WeightId;
                existing.Price = variant.Price;

                _unitOfWork.ProductVariants.Update(existing);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Варіант успішно оновлено";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Помилка при оновленні.");
                await PopulateDropdowns(variant.ProductId, variant.WeightId);
                return View("~/Views/Home/FormVariant.cshtml", variant);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(id);
            if (variant != null)
            {
                try
                {
                    _unitOfWork.ProductVariants.Remove(variant);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Варіант видалено.";
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    TempData["Error"] = "Неможливо видалити цей варіант, оскільки для нього існують партії на складі або замовлення.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Сталася помилка при видаленні.";
                }
            }
            else
            {
                TempData["Error"] = "Варіант не знайдено.";
            }

            return RedirectToAction("Index", "Home");
        }

        private async Task PopulateDropdowns(int? selectedProduct = null, int? selectedWeight = null)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var weights = await _unitOfWork.Weights.GetAllAsync();

            ViewBag.ProductId = new SelectList(products, "ProductId", "Name", selectedProduct);
            ViewBag.WeightId = new SelectList(weights, "WeightId", "WeightValue", selectedWeight);
        }
    }
}