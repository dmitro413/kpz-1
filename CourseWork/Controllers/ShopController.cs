using CourseWork.Data;
using CourseWork.Models;
using CourseWork.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Index(string searchString, int? brandId, int? typeId, int? minRating, string sortOrder = "newest", int page = 1)
        {

            int pageSize = 12;

            var (products, totalCount) = await ((ProductRepository)_unitOfWork.Products).GetShopProductsAsync(searchString, brandId, typeId, sortOrder, page, pageSize, minRating);
            if (minRating.HasValue)
            {
                products = products.Where(p => p.AggregateRating >= minRating.Value);
            }

            ViewBag.Brands = await _unitOfWork.Brands.GetAllAsync();
            ViewBag.Types = await _unitOfWork.TypeOfProducts.GetAllAsync();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBrand = brandId;
            ViewBag.CurrentType = typeId;
            ViewBag.CurrentRating = minRating;

            ViewBag.CurrentPage = page;

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View("IndexSHOP", products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            if (rating != 1 && rating != -1)
            {
                TempData["Error"] = "Некоректна оцінка.";
                return RedirectToAction("Details", new { id = productId });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

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
            catch (Exception)
            {
                TempData["Error"] = "Ви вже залишали відгук на цей товар.";
            }

            return RedirectToAction("Details", new { id = productId });
        }
    }
}