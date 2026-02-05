using CourseWork.Constants;
using CourseWork.Data;
using CourseWork.Models;
using CourseWork.Repositories; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseWork.Services;


namespace CourseWork.Controllers
{
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public class ProductsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileService _fileService;

        public ProductsController(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _fileService = fileService;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View("~/Views/Home/FormProduct.cshtml", new Product());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            ModelState.Remove("Brand");
            ModelState.Remove("TypeOfProduct");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(product.BrandId, product.TypeOfProductId);
                return View("~/Views/Home/FormProduct.cshtml", product);
            }

            product.ImageUrl = await _fileService.SaveProductImageAsync(imageFile);

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = $"Продукт '{product.Name}' створено.";
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            await PopulateDropdowns(product.BrandId, product.TypeOfProductId);
            return View("~/Views/Home/FormProduct.cshtml", product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            ModelState.Remove("imageFile");

            ModelState.Remove("Brand");
            ModelState.Remove("TypeOfProduct");
            ModelState.Remove("ImageUrl");

            ModelState.Remove("Reviews");
            ModelState.Remove("ProductVariants");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(product.BrandId, product.TypeOfProductId);
                return View("~/Views/Home/FormProduct.cshtml", product);
            }

            var existing = await _unitOfWork.Products.GetByIdAsync(product.ProductId);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.BrandId = product.BrandId;
            existing.TypeOfProductId = product.TypeOfProductId;
            existing.CaloriesPer100g = product.CaloriesPer100g;
            existing.Description = product.Description;

            if (imageFile != null && imageFile.Length > 0)
            {
                string oldPath = existing.ImageUrl;
                existing.ImageUrl = await _fileService.SaveProductImageAsync(imageFile);

                _fileService.DeleteFile(oldPath);
            }

            _unitOfWork.Products.Update(existing);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Продукт оновлено.";
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = UserRoles.AdminOrManager)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, bool showDeleted = false)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product != null)
            {
                try
                {
                    _unitOfWork.Products.Remove(product);
                    await _unitOfWork.SaveAsync();
                    
                    TempData["Success"] = "Продукт видалено (soft delete).";      
                    //DeleteImageFile(product.ImageUrl);
                }
                catch (DbUpdateException ex)
                {
                    TempData["Error"] = "Неможливо видалити: продукт використовується в варіантах.";
                }
            }
            else
            {
                TempData["Error"] = "Продукт не знайдено.";
            }
            return RedirectToAction("Index", "Home", new { showDeleted = showDeleted });
        }
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id, bool showDeleted = false)
        {
            var restored = await _unitOfWork.Products.RestoreAsync(id);
            
            if (restored)
            {
                await _unitOfWork.SaveAsync();
                TempData["Success"] = "Продукт успішно відновлено.";
            }
            else
            {
                TempData["Error"] = "Продукт не знайдено або не був видалений.";
            }

            return RedirectToAction("Index", "Home", new { showDeleted = showDeleted });
        }
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdIncludingDeletedAsync(id);


            if (product != null)
            {
                try
                {
                    _fileService.DeleteFile(product.ImageUrl);

                    var reviews = await _unitOfWork.Reviews.GetByProductIdAsync(id); 
                    foreach (var review in reviews)
                    {
                        _unitOfWork.Reviews.Remove(review);
                    }

                    var variants = await _unitOfWork.ProductVariants.GetByProductIdAsync(id);
                    foreach (var variant in variants)
                    {
                        var batches = await _unitOfWork.ProductBatches.GetByVariantIdAsync(variant.VariantId); 
                        foreach (var batch in batches)
                        {
                            _unitOfWork.ProductBatches.Remove(batch);
                        }
                        _unitOfWork.ProductVariants.Remove(variant);
                    }

                    ((ProductRepository)_unitOfWork.Products).HardDelete(product);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Продукт остаточно видалено з бази даних.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Неможливо видалити: продукт використовується в варіантах.";
                }
            }
            else
            {
                TempData["Error"] = "Продукт не знайдено.";
            }

            return RedirectToAction("Index", "Home", new { showDeleted = true });
        }

       

        private async Task PopulateDropdowns(int? brandId = null, int? typeId = null)
        {
            ViewBag.BrandId = new SelectList(
                await _unitOfWork.Brands.GetAllAsync(), 
                "BrandId", 
                "BrandName", 
                brandId
            );
            
            ViewBag.TypeOfProductId = new SelectList(
                await _unitOfWork.TypeOfProducts.GetAllAsync(), 
                "TypeOfProductId", 
                "TypeOfProductName", 
                typeId
            );
        }


    }
}