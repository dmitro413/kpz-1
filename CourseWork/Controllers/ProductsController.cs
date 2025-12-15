using CourseWork.Models;
using CourseWork.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin,Manager")] 
    public class ProductsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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

            product.ImageUrl = await SaveImageAsync(imageFile);

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
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Помилка валідації: {error.ErrorMessage}");
                }

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
                string newPath = await SaveImageAsync(imageFile);

                if (newPath != "/images/no-image.png")
                {
                    string oldPath = existing.ImageUrl;
                    existing.ImageUrl = newPath;

                    DeleteImageFile(oldPath);
                }
            }

            _unitOfWork.Products.Update(existing);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Продукт оновлено.";
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product != null)
            {
                try
                {
                    _unitOfWork.Products.Remove(product);
                    await _unitOfWork.SaveAsync();
                    
                    TempData["Success"] = "Продукт видалено (soft delete).";      
                    DeleteImageFile(product.ImageUrl);
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
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
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
            
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "Admin")] 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var product = await _unitOfWork.Products
                .GetAllIncludingDeletedAsync()
                .ContinueWith(t => t.Result.FirstOrDefault(p => p.ProductId == id));

            if (product != null)
            {
                try
                {
                    DeleteImageFile(product.ImageUrl);
                    _unitOfWork.Products.HardDelete(product);
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
            
            return RedirectToAction("Index", "Home");
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return "/images/no-image.png";
                }
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    return "/images/no-image.png";
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return "/images/products/" + uniqueFileName;
            }

            return "/images/no-image.png";
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl == "/images/no-image.png")
            {
                return;
            }

            try
            {
                string relativePath = imageUrl.TrimStart('/');
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка видалення файлу {imageUrl}: {ex.Message}");
            }
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