using CourseWork.Data;
using CourseWork.Extensions;
using CourseWork.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseWork.Controllers
{
    public class CartController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private const string CartKey = "Cart";

        public CartController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey) ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int variantId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Кількість повинна бути мінімум 1.";
                return RedirectToAction("Index", "Shop"); 
            }
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId);
            if (variant == null) return NotFound();

            var product = await _unitOfWork.Products.GetByIdAsync(variant.ProductId);
            if (product == null || product.IsDeleted)
            {
                TempData["Error"] = "Цей товар більше не доступний.";
                return RedirectToAction("Index", "Shop");
            }
            var weight = await _unitOfWork.Weights.GetByIdAsync(variant.WeightId);

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey) ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(x => x.VariantId == variantId);


            int currentQtyInCart = existingItem?.Quantity ?? 0;
            int totalRequested = currentQtyInCart + quantity;
            int availableStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(variantId);

            if (totalRequested > availableStock)
            {
                TempData["Error"] = $"Неможливо додати. На складі залишилось всього {availableStock} шт.";

                string refererUrl = Request.Headers["Referer"].ToString();
                return !string.IsNullOrEmpty(refererUrl) ? Redirect(refererUrl) : RedirectToAction("Index", "Shop");
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    VariantId = variantId,
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = variant.Price,
                    WeightInfo = $"{weight.WeightValue} {weight.Unit}",
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObject(CartKey, cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int variantId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.VariantId == variantId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject(CartKey, cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartKey);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int variantId, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(x => x.VariantId == variantId);
            if (item == null)
                return RedirectToAction("Index");

            if (quantity <= 0)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject("Cart", cart);
                return RedirectToAction("Index");
            }

            int availableStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(variantId);

            if (quantity > availableStock)
            {
                TempData["Error"] = $"Вибачте, доступно лише {availableStock} шт.";
                item.Quantity = availableStock;
            }
            else
            {
                item.Quantity = quantity;
            }

            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction("Index");
        }
    }
}