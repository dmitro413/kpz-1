using CourseWork.Constants;
using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseWork.Controllers
{
    [Authorize(Roles = UserRoles.AdminOrManager)] 
    public class BatchesController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public BatchesController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateVariantsDropdown();
            var batch = new ProductBatch
            {
                ManufactureDate = DateOnly.FromDateTime(DateTime.Now),
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
                Stock = 10
            };

            return View("~/Views/Home/FormBatches.cshtml", batch);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductBatch batch)
        {
            ModelState.Remove("Variant");

            if (ModelState.IsValid)
            {
                batch.CreatedAt = DateTime.UtcNow;
                batch.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductBatches.AddAsync(batch);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Партію успішно додано.";
                return RedirectToAction("Index", "Home");
            }

            await PopulateVariantsDropdown(batch.VariantId);
            return View("~/Views/Home/FormBatches.cshtml", batch);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var batch = await _unitOfWork.ProductBatches.GetByIdAsync(id);
            if (batch == null) return NotFound();

            await PopulateVariantsDropdown(batch.VariantId);
            return View("~/Views/Home/FormBatches.cshtml", batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductBatch batch)
        {
            ModelState.Remove("Variant"); 

            if (ModelState.IsValid)
            {
                var existing = await _unitOfWork.ProductBatches.GetByIdAsync(batch.BatchId);

                if (existing != null)
                {
                    existing.VariantId = batch.VariantId;
                    existing.ManufactureDate = batch.ManufactureDate;
                    existing.ExpiryDate = batch.ExpiryDate;
                    existing.Stock = batch.Stock;
                    existing.PurchasePrice = batch.PurchasePrice;
                    existing.SupplierName = batch.SupplierName;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.ProductBatches.Update(existing);
                    await _unitOfWork.SaveAsync();

                    TempData["Success"] = "Партію оновлено.";
                    return RedirectToAction("Index", "Home");
                }
            }

            await PopulateVariantsDropdown(batch.VariantId);
            return View("~/Views/Home/FormBatches.cshtml", batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _unitOfWork.ProductBatches.GetByIdAsync(id);
            if (batch != null)
            {
                _unitOfWork.ProductBatches.Remove(batch);
                await _unitOfWork.SaveAsync();
                TempData["Success"] = "Партію видалено.";
            }
            return RedirectToAction("Index", "Home");
        }

        private async Task PopulateVariantsDropdown(int? selectedId = null)
        {
            var variants = await _unitOfWork.ProductVariants.GetVariantsWithDetailsAsync();

            var items = variants.Select(v => new
            {
                v.VariantId,
                DisplayText = $"{v.Product?.Name ?? "Товар"} ({v.Weight?.WeightValue} {v.Weight?.Unit})"
            });

            ViewBag.VariantId = new SelectList(items, "VariantId", "DisplayText", selectedId);
        }
    }
}