using CourseWork.Data;
using CourseWork.Models;
using CourseWork.Repositories; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseWork.Controllers
{
    public class ShopController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public ShopController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string searchString,int? brandId,int? typeId,int? minRating,
        string sortOrder = "newest")
        {
            IEnumerable<Product> products;

            if (brandId.HasValue && minRating.HasValue)
            {
                products = await ((ProductRepository)_unitOfWork.Products).GetByBrandAndRatingSP(brandId.Value, minRating.Value);

                if (!string.IsNullOrEmpty(searchString))
                    products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                products = await ((ProductRepository)_unitOfWork.Products).GetShopProductsAsync(searchString, brandId, typeId, sortOrder);

                if (minRating.HasValue)
                {
                    products = products.Where(p => p.AggregateRating >= minRating.Value);
                }
            }

            ViewBag.Brands = await _unitOfWork.Brands.GetAllAsync();
            ViewBag.Types = await _unitOfWork.TypeOfProducts.GetAllAsync();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBrand = brandId;
            ViewBag.CurrentType = typeId;
            ViewBag.CurrentRating = minRating;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            if (rating != 1 && rating != -1)
            {
                return BadRequest("Некоректна оцінка");
            }

            int userId = 1;

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = (short)rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _unitOfWork.Reviews.AddAsync(review);
                await _unitOfWork.SaveAsync();        
            }
            catch
            {
                TempData["Error"] = "Ви вже залишали відгук на цей товар.";
                return RedirectToAction("Details", new { id = productId });
            }

            TempData["Success"] = "Дякуємо за відгук!";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}